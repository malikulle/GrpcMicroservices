using DiscountGrpc.Data;
using DiscountGrpc.Protos;
using Grpc.Core;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace DiscountGrpc.Services
{
    public class DiscountService : DiscountProtoService.DiscountProtoServiceBase
    {
        private readonly ILogger _logger;

        public DiscountService(ILogger logger)
        {
            _logger = logger;
        }

        public override Task<DiscountModel> GetDiscount(GetDiscountRequest request, ServerCallContext context)
        {
            var discount = DiscountContext.Discounts.FirstOrDefault(x => x.Code == request.DiscountCode);

            return Task.FromResult(new DiscountModel()
            {
                Code = discount.Code,
                Amount = discount.Amount,
                DiscountId = discount.DiscountId
            });
        }
    }
}
