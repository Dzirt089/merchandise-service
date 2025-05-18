using MediatR;

using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents
{
	/// <summary>
	/// Событие, которое возникает при выдаче мерча по запросу
	/// </summary>
	public sealed record MerchandiseRequestGiveOutDomainEvent : INotification
	{
		/// <summary>
		/// Сотрудник, которому выдаем мерч
		/// </summary>
		public Employee Employee { get; init; }

		/// <summary>
		/// Набор мерча, который выдаем сотруднику
		/// </summary>
		public SkuPreset SkuPreset { get; init; }
	}
}
