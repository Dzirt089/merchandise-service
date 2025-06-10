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

			// Собираем и публикуем события домена
			var entities = _context.ChangeTracker
				.Entries<Entity>()
				.Select(x => x.Entity)
				.Where(x => x.DomainEvents.Any())
				.ToList();

			foreach (var entity in entities)
			{
				var domainEvents = entity.DomainEvents.ToList();

				// Публикуем события домена
				//foreach (var domainEvent in domainEvents)
				//{
				//	await _mediator.Publish(domainEvent, cancellationToken);
				//}
				// Пакетная отправка
				var tasks = domainEvents.Select(e => _mediator.Publish(e, cancellationToken));
				await Task.WhenAll(tasks);

				entity.ClearDomainEvents();
			}

			// Сохраняем изменения в БД
			await _context.SaveChangesAsync(cancellationToken);

			return response;
		}
	}
}
//Просто программист, [03.06.2025 20:36]
//Транзакция тут не нужна. НО у вас тут возможны ралли. Вы сначала публикуете события а потом делаете savechanges. И возможна такая ситуация. Что ваша БД по какой-то причине затупит. Консюмер получит событие, Запроси данные у вашего сервиса, а данные еще старые.

//Просто программист, [03.06.2025 20:37]
//И доменные события сначала надо публиковать, а потом уже удалять. Если что-то пойдет не так, вы потеряете их

//Ну и WhenAll не гарантирует вам последовательность публикации событий, а это очень важно. Ведь у вас может быть  EntityCreatedEvent, PropretyChangedEvent, и если они попадут в очередь в другой последовательности будет не хорошо

//У нас вот такая посделовательность бихевиров.
//DomainEventsDispatchingBehavior<,> - собираем доменные события, отдаем их хендлерам
//UnitOfWorkBehavior<,> - сохраняем изменения в БД
//IntegrationEventsDispatchingBehavior<,> - публикуем события которые нагенерили хендлеры доменных событий
//LoggingBehavior<,> - распихиваем логи по разным системам


#region по-штучная отправка
// Публикуем события домена
//foreach (var domainEvent in domainEvents)
//{
//	await _mediator.Publish(domainEvent, cancellationToken);
//}
#endregion