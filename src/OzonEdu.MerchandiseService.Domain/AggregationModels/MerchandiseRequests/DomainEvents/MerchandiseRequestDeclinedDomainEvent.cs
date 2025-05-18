using MediatR;

using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents
{
	/// <summary>
	/// Событие, которое возникает при отклонении заявки на выдачу мерча
	/// </summary>
	public sealed record MerchandiseRequestDeclinedDomainEvent : INotification
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
