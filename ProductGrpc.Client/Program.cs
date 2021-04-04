using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Grpc.Net.Client;
using ProductGrpc.Protos;
using System;
using System.Threading.Tasks;

namespace ProductGrpc.Client
{
    class Program
    {
        static async Task Main(string[] args)
        {
            using var channel = GrpcChannel.ForAddress("https://localhost:5001");
            var client = new ProductProtoService.ProductProtoServiceClient(channel);

            await GetProductAsync(client);
            await AddProductAsync(client);

            await UpdateProductAsync(client);
            await DeleteProductAsync(client);

            await InserBulkProduct(client);
            
            await GetAllProductAsync(client);
            Console.ReadKey();
        }

        private static async Task InserBulkProduct(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("InserBulkProduct started");

            using var clientBulk = client.InsertBulkProduct();

            for (int i = 0; i < 3; i++)
            {
                var productModel = new ProductModel
                {
                    Name = $"product{i}",
                    Description = "Bulk",
                    Price = 399,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                };

                await clientBulk.RequestStream.WriteAsync(productModel);
            }
            await clientBulk.RequestStream.CompleteAsync();

            
        }

        private static async Task DeleteProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("DeleteProductAsync started");


            var deletedProductResponse = await client.DeleteProductAsync(new DeleteProductRequest()
            {
                ProductId = 3
            });
            Console.WriteLine("Deleted Response : ", deletedProductResponse.ToString());

        }

        private static async Task UpdateProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("UpdateProductAsync started");
            var updatedProductResponse = await client.UpdateProductAsync(new UpdateProductRequest()
            {
                Product = new ProductModel()

                {
                    ProductId = 1,
                    Name = "Red",
                    Description = "New Xiaomi Phone Mi10T",
                    Price = 699,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }

            });

            Console.WriteLine("Updated Response : ", updatedProductResponse.ToString());
        }

        private static async Task AddProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("AddProductAsync Started");
            var addedProductResponse = await client.AddProductAsync(new AddProductRequest()
            {
                Product = new ProductModel()
                {
                    Name = "Red",
                    Description = "New Red Phone",
                    Price = 699,
                    Status = ProductStatus.Instock,
                    CreatedTime = Timestamp.FromDateTime(DateTime.UtcNow)
                }
            });
            Console.WriteLine("Added Product : " + addedProductResponse.ToString());
        }

        private static async Task GetAllProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            //Console.WriteLine("GetAllProductAsync Started");
            //using (var clientData = client.GetAllProducts(new GetAllProductRequest()))
            //{
            //    while (await clientData.ResponseStream.MoveNext(new System.Threading.CancellationToken()))
            //    {
            //        var currentProduct = clientData.ResponseStream.Current;
            //        Console.WriteLine(currentProduct);
            //    }
            //}

            Console.WriteLine("GetAllProductAsync With C#9");
            using var clientData = client.GetAllProducts(new GetAllProductRequest());
            await foreach (var responseData in clientData.ResponseStream.ReadAllAsync())
                Console.WriteLine(responseData.ToString());


        }

        private static async Task GetProductAsync(ProductProtoService.ProductProtoServiceClient client)
        {
            Console.WriteLine("GetProductAssync started");
            var response = await client.GetProductAsync(new GetProductRequest() { ProductId = 1 });
            Console.WriteLine("GetProductResponse : " + response.ToString());
        }
    }
}
