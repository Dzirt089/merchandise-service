using MediatR;

using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	/// <summary>
	/// Обработчик события отказа в выдаче мерча
	/// </summary>
	public sealed class MerchandiseRequestDeclinedHandler : INotificationHandler<MerchandiseRequestDeclinedDomainEvent>
	{

		public MerchandiseRequestDeclinedHandler()
		{
		}

		public async Task Handle(MerchandiseRequestDeclinedDomainEvent notification, CancellationToken cancellationToken)
		{
			//await _emailService.SendEmail(notification.Employee.Email, new { Header = "Отказ в выдаче мерча", Body = "К сожалению, мы не можем выдать вам мерч, обратитесь к Вашему HR за разъяснением" });
		}
	}
}
