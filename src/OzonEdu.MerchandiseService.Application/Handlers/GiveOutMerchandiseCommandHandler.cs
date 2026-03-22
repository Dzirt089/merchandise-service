using MediatR;

using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.Application.Commands.GiveOutMerchandise;
using OzonEdu.MerchandiseService.Application.Integration;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;
using OzonEdu.StockApi.Grpc;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
    public sealed class GiveOutMerchandiseCommandHandler : IRequestHandler<GiveOutMerchandiseCommand, bool>
    {
        private const string EmailNotificationTopic = "email_notification_event";
        private readonly IMerchandiseRepository _merchandiseRepository;
        private readonly ISkuPresetRepository _skuPresetRepository;
        private readonly IIntegrationOutboxWriter _integrationOutboxWriter;
        private readonly StockApiGrpc.StockApiGrpcClient _stockApiGrpcClient;
        private readonly ActivitySource _activitySource;

        public GiveOutMerchandiseCommandHandler(
            IMerchandiseRepository merchandiseRepository,
            ISkuPresetRepository skuPresetRepository,
            IIntegrationOutboxWriter integrationOutboxWriter,
            StockApiGrpc.StockApiGrpcClient stockApiGrpcClient = null,
            ActivitySource activitySource = null)
        {
            _merchandiseRepository = merchandiseRepository;
            _skuPresetRepository = skuPresetRepository;
            _integrationOutboxWriter = integrationOutboxWriter;
            _stockApiGrpcClient = stockApiGrpcClient;
            _activitySource = activitySource ?? MerchandiseTelemetry.ActivitySource;
        }

        public async Task<bool> Handle(GiveOutMerchandiseCommand request, CancellationToken cancellationToken)
        {
            using var activity = _activitySource.StartActivity("CommandHandler.GiveOutMerchandise", ActivityKind.Internal);

            var presetType = PresetType.Parse(request.Type);
            var clothingSize = ClothingSize.Parse(request.ClothinSize);

            //Найти SkuPreset
            SkuPreset? skuPreset = await _skuPresetRepository.FindByTypeAsync(presetType, clothingSize, cancellationToken); //+ clothing_size

            //Найти все запросы мерча, которые выдавались сотруднику
            var alreadyExistsRequests = await _merchandiseRepository.GetByEmployeeEmailAsync(Email.Create(request.Email), cancellationToken);

            //Создать запрос на выдачу мерча
            MerchandiseRequest? newMerchandiseRequest;

            newMerchandiseRequest = MerchandiseRequest.Create(
            skuPreset: skuPreset,
            employee: new Employee(email: Email.Create(request.Email), clothingSize: ClothingSize.Parse(request.ClothinSize)),
            alreadyExistedRequest: alreadyExistsRequests,
            createAt: DateTimeOffset.UtcNow);

            //Сохраняем в БД
            await _merchandiseRepository.CreateAsync(newMerchandiseRequest, cancellationToken);

            GiveOutItemsRequest giveOutItems = new GiveOutItemsRequest();
            giveOutItems.Items.AddRange(skuPreset.SkuCollection.Select(x => new SkuQuantityItem { Sku = x.Value, Quantity = 1 }));

            //Забронировать мерч
            GiveOutItemsResponse? skuPackAvailable = await _stockApiGrpcClient
                .GiveOutItemsAsync(giveOutItems, cancellationToken: cancellationToken);

            bool isskuPackAvailable = false;

            if (skuPackAvailable.Result == GiveOutItemsResponse.Types.Result.Successful)
                isskuPackAvailable = true;

            //Выдаем мерч
            var statusRequest = newMerchandiseRequest.GiveOut(isskuPackAvailable, DateTimeOffset.UtcNow);
            await _merchandiseRepository.UpdateAsync(newMerchandiseRequest, cancellationToken);

            if (statusRequest == MerchandiseRequestStatus.Done)
            {
                await _integrationOutboxWriter.AddAsync(
                    EmailNotificationTopic,
                    newMerchandiseRequest.Employee.Email.Value,
                    NotificationEventFactory.CreateMerchDelivery(
                        newMerchandiseRequest.Employee.Email.Value,
                        newMerchandiseRequest.SkuPreset.Type.Id,
                        newMerchandiseRequest.Employee.ClothingSize.Id),
                    cancellationToken);
            }

            return statusRequest == MerchandiseRequestStatus.Done;
        }
    }
}
