using Grpc.Core;
using Grpc.Net.Client;
using IdentityModel.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using ProductGrpc.Protos;
using ShoppingCartGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ShoppingCartWorkerService
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger;
        private readonly IConfiguration _configuration;

        public Worker(ILogger<Worker> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {

                using var scChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ShoppingCartServiceUrl"));
                var scClient = new ShoppingCartProtoService.ShoppingCartProtoServiceClient(scChannel);

                var token = await GetTokenFromIS4();
                _logger.LogInformation("Token {token}", token);

                var scModel = await GetOrCreateShoppingCartAsync(scClient,token);

                using var scClientStream = scClient.AddItemIntoShoppingCart();

                using var productChannel = GrpcChannel.ForAddress(_configuration.GetValue<string>("WorkerService:ProductServiceUrl"));
                var productClient = new ProductProtoService.ProductProtoServiceClient(productChannel);

                using var clientData = productClient.GetAllProducts(new GetAllProductRequest());
                await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                {
                    _logger.LogInformation("GetAllProducts Stream Response : {responseData}", responseData);

                    //var addNewScItem = new AddItemIntoShoppingCartRequest()
                    //{
                    //    Username = _configuration.GetValue<string>("WorkerService:UserName"),
                    //    DiscountCode = "CODE_100",
                    //    NewCartItem = new ShoppingCartItemModel()
                    //    {
                    //        ProductId = responseData.ProductId,
                    //        Productname = responseData.Name,
                    //        Color = "Black",
                    //        Price = responseData.Price,
                    //        Quantity = 1
                    //    }
                    //};
                    //await scClientStream.RequestStream.WriteAsync(addNewScItem);
                }
                //await scClientStream.RequestStream.CompleteAsync();

                //var addItemIntoShoppingCartResponse = await scClientStream;
                //_logger.LogInformation("AddItemIntoShoppingCart ClientStream Response {addItemIntoShoppingCartResponse}", addItemIntoShoppingCartResponse);


                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);
                await Task.Delay(_configuration.GetValue<int>("WorkerService:TaskInterval"), stoppingToken);
            }
        }

        private async Task<string> GetTokenFromIS4()
        {
            var client = new HttpClient();
            var disco = await client.GetDiscoveryDocumentAsync(_configuration.GetValue<string>("WorkerService:IdentityServiceUrl"));
            if (disco.IsError)
            {
                Console.WriteLine(disco.Error);
                return String.Empty;
            }

            var tokenResponse = await client.RequestClientCredentialsTokenAsync(new ClientCredentialsTokenRequest()
            {
                Address = disco.TokenEndpoint,
                ClientId = "ShoppingCartClient",
                ClientSecret = "secret",
                Scope = "ShoppingCartAPI"
            });

            if (tokenResponse.IsError)
            {
                Console.WriteLine(tokenResponse.Error);
                return String.Empty;
            }
            return tokenResponse.AccessToken;
        }

        private async Task<ShoppingCartModel> GetOrCreateShoppingCartAsync(ShoppingCartProtoService.ShoppingCartProtoServiceClient scClient,string token)
        {
            ShoppingCartModel shoppingCartModel;
            var headers = new Metadata();
            headers.Add("Authorization", $"Bearer {token}");
            try
            {
                shoppingCartModel = await scClient.GetShoppingCartAsync(new GetShoppingCartRequest() { Username = _configuration.GetValue<string>("WorkerService:UserName") },headers);

                if(shoppingCartModel == null)                
                    shoppingCartModel = await scClient.CreateShoppingCartAsync(new ShoppingCartModel() { Username = _configuration.GetValue<string>("WorkerService:UserName") },headers);
                
                return shoppingCartModel;
            }
            catch (RpcException ex)
            {
                if(ex.StatusCode == StatusCode.NotFound)
                {
                    shoppingCartModel = await scClient.CreateShoppingCartAsync(new ShoppingCartModel() { Username =  _configuration.GetValue<string>("WorkerService:UserName") },headers);
                    return shoppingCartModel;
                }
            }
            return null;
        }
    }
}
