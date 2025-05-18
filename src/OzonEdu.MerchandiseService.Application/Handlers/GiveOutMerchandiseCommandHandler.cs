using MediatR;

using OzonEdu.MerchandiseService.Application.Commands.GiveOutMerchandise;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;
using OzonEdu.StockApi.Grpc;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class GiveOutMerchandiseCommandHandler : IRequestHandler<GiveOutMerchandiseCommand, bool>
	{
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly ISkuPresetRepository _skuPresetRepository;
		private readonly StockApiGrpc.StockApiGrpcClient _stockApiGrpcClient;

		public GiveOutMerchandiseCommandHandler(
			IMerchandiseRepository merchandiseRepository,
			ISkuPresetRepository skuPresetRepository,
			StockApiGrpc.StockApiGrpcClient stockApiGrpcClient = null)
		{
			_merchandiseRepository = merchandiseRepository;
			_skuPresetRepository = skuPresetRepository;
			_stockApiGrpcClient = stockApiGrpcClient;
		}

		public async Task<bool> Handle(GiveOutMerchandiseCommand request, CancellationToken cancellationToken)
		{
			//Найти SkuPreset
			SkuPreset? skuPreset = await _skuPresetRepository.FindByTypeAsync(PresetType.Parse(request.Type), cancellationToken);

			//Найти все запросы мерча, которые выдавались сотруднику
			var alreadyExistsRequests = await _merchandiseRepository.GetByEmployeeEmailAsync(Email.Create(request.Email), cancellationToken);

			//Создать запрос на выдачу мерча
			MerchandiseRequest? newMerchandiseRequest;
			try
			{
				newMerchandiseRequest = MerchandiseRequest.Create(
				skuPreset: skuPreset,
				employee: new Employee(email: Email.Create(request.Email), clothingSize: ClothingSize.Parse(request.ClothinSize)),
				alreadyExistedRequest: alreadyExistsRequests,
				createAt: DateTimeOffset.UtcNow);
			}
			catch (DomainException)
			{
				//Если не удалось создать запрос, значит выдача мерча невозможна
				return false;
			}

			//Сохраняем в БД
			var newId = await _merchandiseRepository.CreateAsync(newMerchandiseRequest, cancellationToken);


			GiveOutItemsRequest giveOutItems = new GiveOutItemsRequest();
			giveOutItems.Items.AddRange(skuPreset.SkuCollection.Select(x => new SkuQuantityItem { Sku = x.Value, Quantity = 1 }));


			//Забронировать мерч
			var skuPackAvailable = await _stockApiGrpcClient
				.GiveOutItemsAsync(giveOutItems, cancellationToken: cancellationToken);

			bool isskuPackAvailable = false;

			if (skuPackAvailable.Result == GiveOutItemsResponse.Types.Result.Successful)
				isskuPackAvailable = true;

			//Выдаем мерч
			MerchandiseRequestStatus? statusRequest;
			try
			{
				statusRequest = newMerchandiseRequest.GiveOut(isskuPackAvailable, DateTimeOffset.UtcNow);
			}
			catch (DomainException)
			{
				//Если не удалось выдать мерч, значит выдача мерча невозможна
				return false;
			}

			//Обновляем статус заявки
			await _merchandiseRepository.UpdateAsync(newMerchandiseRequest, cancellationToken);

			if (Equals(statusRequest.Name, MerchandiseRequestStatus.Done))
			{
				//Отправляем уведомление на почту
				//await _emailService.SendEmail(newMerchandiseRequest.Employee.Email, new object());
			}

			return true;
		}
	}
}
