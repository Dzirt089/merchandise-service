using MediatR;

using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents
{
	/// <summary>
	/// Событие, которое возникает при выдаче мерча по запросу
	/// </summary>
	public sealed class MerchandiseRequestGiveOutDomainEvent : INotification
	{
		/// <summary>
		/// Сотрудник, которому выдаем мерч
		/// </summary>
		public Employee Employee { get; set; }

		/// <summary>
		/// Набор мерча, который выдаем сотруднику
		/// </summary>
		public SkuPreset SkuPreset { get; set; }
	}
}
