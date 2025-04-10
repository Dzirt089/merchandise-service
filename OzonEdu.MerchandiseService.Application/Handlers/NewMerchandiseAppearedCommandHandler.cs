using MediatR;

using OzonEdu.MerchandiseService.Application.Commands.NewMerchandiseAppeared;
using OzonEdu.MerchandiseService.Application.Contracts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.Interfaces;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public class NewMerchandiseAppearedCommandHandler : IRequestHandler<NewMerchandiseAppearedCommand>
	{
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly IStockApiIntegration _stockApiIntegration;

		public NewMerchandiseAppearedCommandHandler(
			IMerchandiseRepository merchandiseRepository,
			IStockApiIntegration stockApiIntegration)
		{
			_merchandiseRepository = merchandiseRepository;
			_stockApiIntegration = stockApiIntegration;
		}

		public async Task Handle(NewMerchandiseAppearedCommand request, CancellationToken cancellationToken)
		{
			IReadOnlyCollection<MerchandiseRequest>? allProcessingRequest = await _merchandiseRepository.GetAllProcessingRequestsAsync(cancellationToken);

			allProcessingRequest = allProcessingRequest
				.Where(x => x.SkuPreset.SkuCollection.All(z => request.SkuCollection.Contains(z.Value)))
				.OrderBy(x => x.CreatedAt)
				.ToList();

			foreach (var merchandiseRequest in allProcessingRequest)
			{
				var isAvailable =
					await _stockApiIntegration.RequestGiveOutAsync(merchandiseRequest.SkuPreset.SkuCollection.Select(x => x.Value),
					cancellationToken);

				if (isAvailable)
				{
					merchandiseRequest.GiveOut(isAvailable, DateTimeOffset.UtcNow);

					await _merchandiseRepository.UpdateAsync(merchandiseRequest, cancellationToken);
				}

			}
		}
	}
}
