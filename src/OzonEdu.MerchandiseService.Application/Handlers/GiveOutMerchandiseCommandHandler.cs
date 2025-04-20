using MediatR;

using OzonEdu.MerchandiseService.Application.Commands.GiveOutMerchandise;
using OzonEdu.MerchandiseService.Application.Contracts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.Interfaces;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets.Interfaces;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class GiveOutMerchandiseCommandHandler : IRequestHandler<GiveOutMerchandiseCommand, bool>
	{
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly ISkuPresetRepository _skuPresetRepository;
		private readonly IStockApiIntegration _stockApiIntegration;
		private readonly IEmailService _emailService;
		public GiveOutMerchandiseCommandHandler(
			IMerchandiseRepository merchandiseRepository,
			ISkuPresetRepository skuPresetRepository,
			IStockApiIntegration stockApiIntegration,
			IEmailService emailService)
		{
			_merchandiseRepository = merchandiseRepository;
			_skuPresetRepository = skuPresetRepository;
			_stockApiIntegration = stockApiIntegration;
			_emailService = emailService;
		}

		public async Task<bool> Handle(GiveOutMerchandiseCommand request, CancellationToken cancellationToken)
		{
			//Найти SkuPreset
			var skuPreset = await _skuPresetRepository.FindByTypeAsync(PresetType.Parse(request.Type), cancellationToken);

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

			newMerchandiseRequest = new MerchandiseRequest(
				id: newId,
				skuPreset: newMerchandiseRequest.SkuPreset,
				employee: newMerchandiseRequest.Employee,
				status: newMerchandiseRequest.Status,
				createdAt: newMerchandiseRequest.CreatedAt,
				giveOutAt: newMerchandiseRequest.GiveOutAt);

			//Забронировать мерч
			var isskuPackAvailable = await _stockApiIntegration
				.RequestGiveOutAsync(skuPreset.SkuCollection.Select(x => x.Value), cancellationToken);

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
				await _emailService.SendEmail(newMerchandiseRequest.Employee.Email, new object());
			}

			return true;
		}
	}
}
