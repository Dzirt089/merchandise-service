using MediatR;

namespace OzonEdu.MerchandiseService.Domain.Root
{
	/// <summary>
	/// Сущности с изменяемыми свойствами и с уникальным ID, сравнение происходит по ID.
	/// Частично шаблон взят отсюда
	/// https://learn.microsoft.com/ru-ru/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
	/// Entity может в себе содержать св-ва других ValueObject, так и св-ва Entity
	/// </summary>
	public abstract class Entity
	{

		/// <summary>
		/// Хэш-код сущности
		/// </summary>
		protected int? _requestedHashCode;

		/// <summary>
		/// Идентификатор сущности
		/// </summary>
		public virtual long Id { get; protected set; }

		/// <summary>
		/// Список доменных событий
		/// </summary>
		private readonly List<INotification> _domainEvents = new();

		/// <summary>
		/// Список доменных событий
		/// </summary>
		public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();

		/// <summary>
		/// Добавить доменное событие
		/// </summary>
		/// <param name="eventItem"></param>
		public void AddDomainEvent(INotification eventItem)
		{
			_domainEvents.Add(eventItem);
		}

		/// <summary>
		/// Удалить доменное событие
		/// </summary>
		/// <param name="eventItem"></param>
		public void RemoveDomainEvent(INotification eventItem)
		{
			_domainEvents.Remove(eventItem);
		}

		/// <summary>
		/// Очистить список доменных событий
		/// </summary>
		public void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}

		/// <summary>
		/// Проверка на транзиентность
		/// </summary>
		/// <returns></returns>
		public bool IsTransient()
		{
			return Id == default(long);
		}

		/// <summary>
		/// Сравнение сущностей
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object? obj)
		{
			if (obj is not Entity entity) return false;
			if (ReferenceEquals(this, entity)) return true;
			if (GetType() != entity.GetType()) return false;
			if (entity.IsTransient() || IsTransient()) return false;
			else return entity.Id == Id;
		}

		/// <summary>
		/// Получение хэш-кода
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			if (!IsTransient())
			{
				if (!_requestedHashCode.HasValue) _requestedHashCode = HashCode.Combine(Id, 31);
				return _requestedHashCode.Value;
			}
			else return base.GetHashCode();
		}

		/// <summary>
		/// Переопределение оператора ==
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator ==(Entity left, Entity right)
		{
			if (Equals(left, null)) return Equals(right, null) ? true : false;
			else return left.Equals(right);
		}

		/// <summary>
		/// Переопределение оператора !=
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		public static bool operator !=(Entity left, Entity right)
		{
			return !(left == right);
		}
	}
}
