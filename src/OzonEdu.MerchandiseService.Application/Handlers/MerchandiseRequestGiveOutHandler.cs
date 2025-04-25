using MediatR;

using OzonEdu.MerchandiseService.Application.Contracts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	/// <summary>
	/// Обработчик события выдачи мерча
	/// </summary>
	public sealed class MerchandiseRequestGiveOutHandler : INotificationHandler<MerchandiseRequestGiveOutDomainEvent>
	{
		private readonly IEmailService _emailService;

		public MerchandiseRequestGiveOutHandler(IEmailService emailService)
		{
			_emailService = emailService;
		}

		public async Task Handle(MerchandiseRequestGiveOutDomainEvent notification, CancellationToken cancellationToken)
		{
			await _emailService.SendEmail(notification.Employee.Email, new { Header = "Выдача мерча", Body = "Подойдите к своему HR" });
		}
	}
}
