using MediatR;

namespace OzonEdu.MerchandiseService.Application.Commands.NewMerchandiseAppeared
{
	/// <summary>
	/// Прибыл новый набор мерча
	/// </summary>
	public record NewMerchandiseAppearedCommand : IRequest
	{
		/// <summary>
		/// Набор доступных артикулов
		/// </summary>
		public IReadOnlyCollection<long> SkuCollection { get; init; }
	}
}
