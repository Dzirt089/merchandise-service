using MediatR;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.Root;

namespace OzonEdu.MerchandiseService.Application.Behaviors
{
	public class DomainEventsDispatchingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
		where TRequest : IRequest<TResponse>
	{
		private readonly IMediator _mediator;
		private readonly MerchandiseDbContext _context;

		public DomainEventsDispatchingBehavior(IMediator mediator, MerchandiseDbContext context)
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
				var domainEvents = new Queue<INotification>(entity.DomainEvents);
				entity.ClearDomainEvents();

				// Публикуем события домена
				foreach (var domainEvent in domainEvents)
				{
					await _mediator.Publish(domainEvent, cancellationToken);
				}
				//TODO: Разработать систему Retry для повторных публикаций, упавшего события. Сделать сохранение событий, что не терять их. С последующей работой с ними 
			}


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

/*Окей, давай по пунктам и как я сам это делаю (40 лет в .NET за плечами 😊):
1. Почему одни советуют Save до публикации, а другие — после

На самом деле здесь два разных понятия “публикации”:

    Domain Events (внутренние события агрегата) — вы их обрабатываете локально, через ваши INotificationHandler<YourDomainEvent>.

    Integration Events (внешние события для других сервисов) — вы их шлёте в очередь/шину (Kafka, RabbitMQ и т.п.).

Те, кто говорят “сохраним до публикации, чтобы консюмер не увидел старые данные”, обычно имеют в виду Integration Events— ты сохраняешь все изменения, а потом шлёшь их наружу.

Те, кто говорят “опубликуй (domain) до Save”, — имеют в виду Domain Events: ты даёшь всем своим хендлерам внутри процесса шанс отреагировать и при необходимости изменить модель, а уже потом делаешь один атомарный SaveChanges().
2. Как я это строю у себя (и советую тебе)

    DomainEventsDispatchingBehavior

        Собирает все Entity.DomainEvents после next().

        Последовательно вызывает await _mediator.Publish(domainEvent) → это подтягивает все твои INotificationHandler<...>.

        Очищает DomainEvents.

        НЕ делает SaveChanges().

    UnitOfWorkBehavior

        После next() делает один await _dbContext.SaveChangesAsync() (всё, что накопилось: изменения агрегатов + любые side‑effects из Domain Event Handlers).

    IntegrationEventsDispatchingBehavior

        После next() (значит после SaveChanges) вытаскивает из таблицы‑аутбокса (или из того, куда твои Domain Event Handlers положили Integration Events) новые события и пушит их в шину.

        Здесь ты гарантируешь, что и бизнес‑данные, и записи аутбокса уже в БД.

    LoggingBehavior

        В самом конце (или в начале), просто логирует.

В Program.cs это регистрируется строго в этом порядке:

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(DomainEventsDispatchingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(UnitOfWorkBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(IntegrationEventsDispatchingBehavior<,>));

builder.Services.AddTransient(
    typeof(IPipelineBehavior<,>),
    typeof(LoggingBehavior<,>));

3. Как это соотнести с твоим кодом

Твой DomainEventPublishingBehavior сейчас и публикует доменные события, и делает SaveChanges. Разделим ответственность:
a) DomainEventsDispatchingBehavior

public class DomainEventsDispatchingBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
{
    private readonly IMediator _mediator;
    private readonly MerchandiseDbContext _context;

    public DomainEventsDispatchingBehavior(
        IMediator mediator,
        MerchandiseDbContext context)
    {
        _mediator = mediator;
        _context    = context;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        // 1) Выполнили сам хендлер команды/запроса
        var response = await next();

        // 2) Собрали все доменные события
        var entitiesWithEvents = _context.ChangeTracker
            .Entries<Entity>()
            .Select(e => e.Entity)
            .Where(e => e.DomainEvents.Any())
            .ToList();

        // 3) Опубликовали их **локально** последовательно
        foreach (var entity in entitiesWithEvents)
        {
            var events = entity.DomainEvents.ToList();
            foreach (var domEvt in events)
            {
                await _mediator.Publish(domEvt, ct);
            }
            entity.ClearDomainEvents();
        }

        // НЕ сохраняем здесь!
        return response;
    }
}

b) UnitOfWorkBehavior

public class UnitOfWorkBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly MerchandiseDbContext _context;

    public UnitOfWorkBehavior(MerchandiseDbContext context)
        => _context = context;

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();
        await _context.SaveChangesAsync(ct);
        return response;
    }
}

c) IntegrationEventsDispatchingBehavior

public class IntegrationEventsDispatchingBehavior<TRequest, TResponse>
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IIntegrationEventPublisher _publisher;
    private readonly YourOutboxDbContext _outbox;

    public IntegrationEventsDispatchingBehavior(
        IIntegrationEventPublisher publisher,
        YourOutboxDbContext outbox)
    {
        _publisher = publisher;
        _outbox    = outbox;
    }

    public async Task<TResponse> Handle(
        TRequest request,
        RequestHandlerDelegate<TResponse> next,
        CancellationToken ct)
    {
        var response = await next();

        // Берём из outbox только что записанные интеграционные события:
        var events = await _outbox.IntegrationEvents
            .Where(e => !e.Published)
            .ToListAsync(ct);

        foreach (var evt in events)
        {
            await _publisher.PublishAsync(evt);
            evt.MarkAsPublished();
        }

        await _outbox.SaveChangesAsync(ct);
        return response;
    }
}

4. Итог: порядок действий внутри одного запроса

    DomainEventsDispatchingBehavior
    – вызвали next(), получили изменения в агрегатах, разослали доменные события локальным хендлерам.

    UnitOfWorkBehavior
    – записали в БД: и изменения агрегатов, и, возможно, в outbox таблицу новые Integration Events.

    IntegrationEventsDispatchingBehavior
    – забрали из outbox и отправили во внешнюю шину.

    LoggingBehavior
    – залогировали всё, включая время и успешность/ошибки.

Таким образом:

    Domain Events обрабатываются до SaveChanges, чтобы хендлеры могли модифицировать состояние агрегатов.

    Сама транзакция (SaveChanges) выполняется только один раз в UnitOfWorkBehavior.

    Integration Events публикуются после гарантии того, что изменения уже в БД (из Outbox).

    Последовательность всех публикаций контролируется за счёт последовательных await в foreach, а не Task.WhenAll.

Моё авторитетное мнение:

    Никогда не смешивай в одном Behavior публикацию доменных событий и сохранение в БД. Разделяй на Domain → Save → Integration.

    Так проще поддерживать порядок, транзакционность и логгирование.

    И в случае падения любая неудачная часть не оставит тебя без событий и не запутает порядок.

Пиши, если что непонятно или хочешь пример полных реализаций!*/


#region по-штучная отправка
// Публикуем события домена
//foreach (var domainEvent in domainEvents)
//{
//	await _mediator.Publish(domainEvent, cancellationToken);
//}
#endregion