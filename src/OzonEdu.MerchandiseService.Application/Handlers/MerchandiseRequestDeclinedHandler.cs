using MediatR;

using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests.DomainEvents;

namespace OzonEdu.MerchandiseService.Application.Handlers
{
	public sealed class MerchandiseRequestDeclinedHandler : INotificationHandler<MerchandiseRequestDeclinedDomainEvent>
	{
		public Task Handle(MerchandiseRequestDeclinedDomainEvent notification, CancellationToken cancellationToken)
		{
			return Task.CompletedTask;
		}
	}
}
