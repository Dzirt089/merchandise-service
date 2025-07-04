﻿using MediatR;

using OzonEdu.MerchandiseService.Application.Commands.NewMerchandiseAppeared;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.StockApi.Grpc;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class NewMerchandiseAppearedCommandHandler : IRequestHandler<NewMerchandiseAppearedCommand>
	{
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly StockApiGrpc.StockApiGrpcClient _stockApiGrpcClient;

		public NewMerchandiseAppearedCommandHandler(
			IMerchandiseRepository merchandiseRepository, StockApiGrpc.StockApiGrpcClient stockApiGrpcClient = null)
		{
			_merchandiseRepository = merchandiseRepository;
			_stockApiGrpcClient = stockApiGrpcClient;
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
				GiveOutItemsRequest giveOutItems = new GiveOutItemsRequest();
				giveOutItems.Items.AddRange(merchandiseRequest.SkuPreset.SkuCollection.Select(x => new SkuQuantityItem { Sku = x.Value, Quantity = 1 }));

				var available =
					await _stockApiGrpcClient.GiveOutItemsAsync(giveOutItems, cancellationToken: cancellationToken);

				bool isAvailable = false;

				if (available.Result == GiveOutItemsResponse.Types.Result.Successful)
					isAvailable = true;

				if (isAvailable)
				{
					merchandiseRequest.GiveOut(isAvailable, DateTimeOffset.UtcNow);

					await _merchandiseRepository.UpdateAsync(merchandiseRequest, cancellationToken);
				}
			}
		}
	}
}
