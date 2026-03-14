using MediatR;

using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.Application.Commands.NewMerchandiseAppeared;
using OzonEdu.MerchandiseService.Application.Integration;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;
using OzonEdu.StockApi.Grpc;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class NewMerchandiseAppearedCommandHandler : IRequestHandler<NewMerchandiseAppearedCommand, Unit>
	{
		private const string EmailNotificationTopic = "email_notification_event";
		private readonly ActivitySource _activitySource;
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly IIntegrationOutboxWriter _integrationOutboxWriter;
		private readonly StockApiGrpc.StockApiGrpcClient _stockApiGrpcClient;

		public NewMerchandiseAppearedCommandHandler(
			IMerchandiseRepository merchandiseRepository,
			IIntegrationOutboxWriter integrationOutboxWriter,
			StockApiGrpc.StockApiGrpcClient stockApiGrpcClient = null,
			ActivitySource activitySource = null)
		{
			_merchandiseRepository = merchandiseRepository;
			_integrationOutboxWriter = integrationOutboxWriter;
			_stockApiGrpcClient = stockApiGrpcClient;
			_activitySource = activitySource ?? MerchandiseTelemetry.ActivitySource;
		}

		public async Task<Unit> Handle(NewMerchandiseAppearedCommand request, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("CommandHandler.NewMerchandiseAppeared", ActivityKind.Internal);

			IReadOnlyCollection<MerchandiseRequest>? allProcessingRequest = await _merchandiseRepository.GetAllProcessingRequestsAsync(cancellationToken);

			allProcessingRequest = allProcessingRequest
				.Where(x => x.SkuPreset.SkuCollection.Any(z => request.SkuCollection.Contains(z.Value)))
				.OrderBy(x => x.CreatedAt)
				.ToList();

			foreach (var merchandiseRequest in allProcessingRequest)
			{
				GiveOutItemsRequest giveOutItems = new GiveOutItemsRequest();
				giveOutItems.Items.AddRange(merchandiseRequest.SkuPreset.SkuCollection.Select(x => new SkuQuantityItem { Sku = x.Value, Quantity = 1 }));

				var available =
					await _stockApiGrpcClient.GiveOutItemsAsync(giveOutItems, cancellationToken: cancellationToken);

				bool isAvailable = false;

				if (available.Result == GiveOutItemsResponse.Types.Result.Successful)
					isAvailable = true;

				if (isAvailable)
				{
					var status = merchandiseRequest.GiveOut(isAvailable, DateTimeOffset.UtcNow);

					await _merchandiseRepository.UpdateAsync(merchandiseRequest, cancellationToken);

					if (status == MerchandiseRequestStatus.Done)
					{
						await _integrationOutboxWriter.AddAsync(
							EmailNotificationTopic,
							merchandiseRequest.Employee.Email.Value,
							NotificationEventFactory.CreateMerchDelivery(
								merchandiseRequest.Employee.Email.Value,
								merchandiseRequest.SkuPreset.Type.Id,
								merchandiseRequest.Employee.ClothingSize.Id),
							cancellationToken);
					}
				}
			}

			return Unit.Value;
		}
	}
}
