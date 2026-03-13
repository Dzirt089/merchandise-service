using MediatR;

using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.Application.Commands.ProcessEmployeeNotification;
using OzonEdu.MerchandiseService.Application.Integration;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;
using OzonEdu.StockApi.Grpc;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class ProcessEmployeeNotificationCommandHandler : IRequestHandler<ProcessEmployeeNotificationCommand, bool>
	{
		private const string EmailNotificationTopic = "email_notification_event";
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly ISkuPresetRepository _skuPresetRepository;
		private readonly IIntegrationOutboxWriter _integrationOutboxWriter;
		private readonly StockApiGrpc.StockApiGrpcClient _stockApiGrpcClient;
		private readonly ActivitySource _activitySource;

		public ProcessEmployeeNotificationCommandHandler(
			IMerchandiseRepository merchandiseRepository,
			ISkuPresetRepository skuPresetRepository,
			IIntegrationOutboxWriter integrationOutboxWriter,
			StockApiGrpc.StockApiGrpcClient stockApiGrpcClient,
			ActivitySource activitySource = null)
		{
			_merchandiseRepository = merchandiseRepository;
			_skuPresetRepository = skuPresetRepository;
			_integrationOutboxWriter = integrationOutboxWriter;
			_stockApiGrpcClient = stockApiGrpcClient;
			_activitySource = activitySource ?? MerchandiseTelemetry.ActivitySource;
		}

		public async Task<bool> Handle(ProcessEmployeeNotificationCommand request, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("CommandHandler.ProcessEmployeeNotification", ActivityKind.Internal);

			var presetType = PresetType.Parse(request.Type);
			var clothingSize = ClothingSize.Parse(request.ClothingSize);

			var skuPreset = await _skuPresetRepository.FindByTypeAsync(presetType, clothingSize, cancellationToken);
			var alreadyExistsRequests = await _merchandiseRepository.GetByEmployeeEmailAsync(Email.Create(request.Email), cancellationToken);

			var merchandiseRequest = MerchandiseRequest.Create(
				skuPreset: skuPreset,
				employee: new Employee(email: Email.Create(request.Email), clothingSize: clothingSize),
				alreadyExistedRequest: alreadyExistsRequests,
				createAt: DateTimeOffset.UtcNow);

			await _merchandiseRepository.CreateAsync(merchandiseRequest, cancellationToken);

			var giveOutItems = new GiveOutItemsRequest();
			giveOutItems.Items.AddRange(skuPreset.SkuCollection.Select(x => new SkuQuantityItem
			{
				Sku = x.Value,
				Quantity = 1
			}));

			var stockResponse = await _stockApiGrpcClient.GiveOutItemsAsync(giveOutItems, cancellationToken: cancellationToken);
			var isAvailable = stockResponse.Result == GiveOutItemsResponse.Types.Result.Successful;

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

			return status == MerchandiseRequestStatus.Done;
		}
	}
}
