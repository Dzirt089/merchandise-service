using MediatR;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.Root;

namespace OzonEdu.MerchandiseService.Application.Behaviors
{
	public class DomainEventPublishingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IMediator _mediator;
		private readonly MerchandiseDbContext _context;

		public DomainEventPublishingBehavior(IMediator mediator, MerchandiseDbContext context)
		{
			_mediator = mediator;
			_context = context;
		}

		public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
		{
			// Выполняем запрос
			var response = await next();

			// Сохраняем изменения в БД
			await _context.SaveChangesAsync(cancellationToken);

			// Собираем и публикуем события домена
			var entities = _context.ChangeTracker
				.Entries<Entity>()
				.Select(x => x.Entity)
				.Where(x => x.DomainEvents.Any())
				.ToList();

			foreach (var entity in entities)
			{
				var domainEvents = entity.DomainEvents.ToList();
				entity.ClearDomainEvents();

				// Публикуем события домена
				//foreach (var domainEvent in domainEvents)
				//{
				//	await _mediator.Publish(domainEvent, cancellationToken);
				//}

				// Пакетная отправка
				var tasks = domainEvents.Select(e => _mediator.Publish(e, cancellationToken));
				await Task.WhenAll(tasks);
			}

			return response;
		}
	}
}
