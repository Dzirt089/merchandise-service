using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using OzonEdu.MerchandiseService.Grpc;
using OzonEdu.MerchandiseService.Services;

namespace OzonEdu.MerchandiseService.GrpcServices
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
				MerchItems = {
					merchItems.Select(x=>new GetAllMerchandiseItemsResponseUnit
					{
						ItemId = x.Id,
						ItemName = x.ItemName,
						ItemQuantity = x.Quantity,
					})}
			};
		}

		public override async Task<GetAllMerchandiseItemsResponse> GetAllMerchandiseITemsV2(
			Empty request,
			ServerCallContext context)
		{
			var merchItems = await _merchService.GetAll(context.CancellationToken);
			return new GetAllMerchandiseItemsResponse
			{
				MerchItems =
				{
					merchItems.Select(x => new GetAllMerchandiseItemsResponseUnit
					{
						ItemId = x.Id,
						ItemName = x.ItemName,
						ItemQuantity = x.Quantity,
					})
				}
			};
		}

		public override async Task<GetOneMerchaniseItemResponse> GetOneMerchaniseItem(
			GetOneMerchaniseItemRequest request,
			ServerCallContext context)
		{
			var merchItem = await _merchService.GetById(request.ItemId, context.CancellationToken);
			return new GetOneMerchaniseItemResponse
			{
				MerchItem = new GetAllMerchandiseItemsResponseUnit
				{
					ItemId = merchItem.Id,
					ItemName = merchItem.ItemName,
					ItemQuantity = merchItem.Quantity,
				}
			};
		}
	}
}
