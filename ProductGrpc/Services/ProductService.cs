using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using Grpc.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ProductGrpc.Data;
using ProductGrpc.Models;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Services
{
    public class ProductService : ProductProtoService.ProductProtoServiceBase
    {
        private readonly ProductContext _productContext;
        private readonly IMapper _mapper;
        private readonly ILogger<ProductService> _logger;

        public ProductService(ProductContext productContext, ILogger<ProductService> logger, IMapper mapper)
        {
            _productContext = productContext;
            _logger = logger;
            _mapper = mapper;
        }

        public override Task<Empty> Test(Empty request, ServerCallContext context)
        {
            return base.Test(request, context);
        }

        public override async Task<ProductModel> GetProduct(GetProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Product.FindAsync(request.ProductId);

            if (product == null)
            {
                throw new RpcException(new Status(StatusCode.NotFound,"Product Not Found"));
            }

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override  async Task GetAllProducts(GetAllProductRequest request, IServerStreamWriter<ProductModel> responseStream, ServerCallContext context)
        {
            var productList = await _productContext.Product.ToListAsync();

            foreach (var product in productList)
            {
                var productModel = _mapper.Map<ProductModel>(product);
                await responseStream.WriteAsync(productModel);
            }
        }

        public override async Task<ProductModel> AddProduct(AddProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            await _productContext.AddAsync(product);
            await _productContext.SaveChangesAsync();

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override async Task<ProductModel> UpdateProduct(UpdateProductRequest request, ServerCallContext context)
        {
            var product = _mapper.Map<Product>(request.Product);

            bool isExist = await _productContext.Product.AnyAsync(x => x.ProductId == product.ProductId);

            if (!isExist)
            {
                // throw ex
            }

            _productContext.Entry(product).State = EntityState.Modified;
            await _productContext.SaveChangesAsync();

            var productModel = _mapper.Map<ProductModel>(product);
            return productModel;
        }

        public override async Task<DeleteProductResponse> DeleteProduct(DeleteProductRequest request, ServerCallContext context)
        {
            var product = await _productContext.Product.FindAsync(request.ProductId);

            if (product == null)
            {
                // throw ex
                throw new RpcException(new Status(StatusCode.NotFound, "Product Not Found"));
            }

            _productContext.Product.Remove(product);
            var deleteCount = await _productContext.SaveChangesAsync();

            var response = new DeleteProductResponse
            {
                Success  = deleteCount > 0
            };

            return response;
        }

        public override async Task<InsertBulkProductResponse> InsertBulkProduct(IAsyncStreamReader<ProductModel> requestStream, ServerCallContext context)
        {
            while (await requestStream.MoveNext())
            {
                var product = _mapper.Map<Product>(requestStream.Current);
                _productContext.Product.Add(product);
            }
            var instertedCount = await _productContext.SaveChangesAsync();

            var response = new InsertBulkProductResponse()
            {
                Success = instertedCount > 0,
                InsertCount = instertedCount
            };
            return response;
        }
    }
}
