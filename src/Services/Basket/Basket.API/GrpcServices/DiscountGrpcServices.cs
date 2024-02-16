using Discount.Grpc.Protos;

namespace Basket.API.GrpcServices
{
    public class DiscountGrpcServices
    {
        private readonly DiscountProtoService.DiscountProtoServiceClient _discountProtoService;

        public DiscountGrpcServices(DiscountProtoService.DiscountProtoServiceClient discountProtoService)
        {
            _discountProtoService = discountProtoService ?? throw new ArgumentNullException(nameof(discountProtoService));
        }

        public async Task<CouponModel> GetDiscount(string productName)
        {
            return await _discountProtoService.GetDiscountAsync(new GetDiscountRequest { ProductName = productName });
        }
    }
}
