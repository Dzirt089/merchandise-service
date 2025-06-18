using MediatR;

using OzonEdu.MerchandiseService.Application.Models.DTOs;
using OzonEdu.MerchandiseService.Application.Queries.GetRequestsByEmployee;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class GetRequestsByEmployeeQueryHandler : IRequestHandler<GetRequestsByEmployeeQuery, GetRequestsByEmployeeQueryResponse>
	{
		private readonly IMerchandiseRepository _merchandiseRepository;
		private readonly ActivitySource _activitySource;

		public GetRequestsByEmployeeQueryHandler(IMerchandiseRepository merchandiseRepository, ActivitySource activitySource = null)
		{
			_merchandiseRepository = merchandiseRepository;
			_activitySource = activitySource;
		}

		/// <summary>
		/// Обработчик запроса на получение всех запросов на получение мерча для сотрудника
		/// </summary>
		public async Task<GetRequestsByEmployeeQueryResponse> Handle(GetRequestsByEmployeeQuery request, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("QueryHandler.GetRequestsByEmployee", ActivityKind.Internal);

			// Получаем все заявки на выдачу мерча по email сотрудника
			var requests =
				await _merchandiseRepository.GetByEmployeeEmailAsync(Email.Create(request.Email), cancellationToken);

			// Маппим нашу доменную сущность в DTO-шки, которые будем отдавать наружу
			return new GetRequestsByEmployeeQueryResponse
			{
				Items = requests.Select(x => new MerchandiseRequestDataDto
				{
					Status = x.Status.Name,
					Type = x.SkuPreset.Type.Name,
					CreatedAt = x.CreatedAt,
					GiveOutAt = x.GiveOutAt
				}).ToList()
			};
		}
	}
}
