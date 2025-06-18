using Google.Protobuf.WellKnownTypes;

using Grpc.Core;

using MediatR;

using OzonEdu.MerchandiseService.Application.Commands.GiveOutMerchandise;
using OzonEdu.MerchandiseService.Application.Queries.GetRequestsByEmployee;
using OzonEdu.MerchandiseService.Grpc;

namespace OzonEdu.MerchandiseService.GrpcServices
{
	public class MerchApiGrpsService : MerchandiseServiceGrpc.MerchandiseServiceGrpcBase
	{
		private readonly IMediator _mediator;

		public MerchApiGrpsService(IMediator merchService)
		{
			_mediator = merchService;
		}

		public override async Task<GetRequestsByEmployeeResponse>
			GetRequestsByEmployee(
			GetRequestsByEmployeeRequest request,
			ServerCallContext context)
		{
			GetRequestsByEmployeeQueryResponse? result = await _mediator.Send(new GetRequestsByEmployeeQuery() { Email = request.Email }, context.CancellationToken);

			var response = new GetRequestsByEmployeeResponse();

			foreach (var item in result.Items)
			{
				var unitResponse = new GetRequestsByEmployeeResponseUnit
				{
					Type = item.Type,
					Status = item.Status,
					CreatedAt = Timestamp.FromDateTimeOffset(item.CreatedAt), // Convert DateTimeOffset to Timestamp
				};

				if (item.GiveOutAt.HasValue)
					unitResponse.GiveOutAt = Timestamp.FromDateTimeOffset((DateTimeOffset)item.GiveOutAt);

				response.Requests.Add(unitResponse);
			}

			return response;
		}

		public override async Task<GiveOutMerchandiseResponse>
			GiveOutMerchandise(
			GiveOutMerchandiseRequest request,
			ServerCallContext context)
		{
			var command = new GiveOutMerchandiseCommand
			{
				ClothinSize = request.MerchRequestUnit.ClothingSize,
				Email = request.MerchRequestUnit.Email,
				Type = request.MerchRequestUnit.Type
			};

			var result = await _mediator.Send(command, context.CancellationToken);
			return new GiveOutMerchandiseResponse { ResponseCheck = result };
		}
	}
}
