using Grpc.Core;
using OzonEdu.MerchandiseService.Grpc;
namespace OzonEdu.MerchandiseService.Services
{
    public class MerchApiGrpsService : MerchandiseServiceGrpc.MerchandiseServiceGrpcBase
    {
        private readonly IMerchService _merchService;

        public MerchApiGrpsService(IMerchService merchService)
        {
            _merchService = merchService;
        }

        public override async Task<GetAllMerchandiseItemsResponse> GetAllMerchandiseITems(
            GetAllMerchandiseItemsRequest request, 
            ServerCallContext context)
        {
            var merchItems = await _merchService.GetAll(context.CancellationToken);
            return new GetAllMerchandiseItemsResponse
            {
                MerchItems = { merchItems.Select(x=>new GetAllMerchandiseItemsResponseUnit
                {
                    ItemId = x.Id,
                    ItemName = x.ItemName,
                    ItemQuantity = x.Quantity,
                })}
            };
            //return base.GetAllMerchandiseITems(request, context);
        }
    }
}
