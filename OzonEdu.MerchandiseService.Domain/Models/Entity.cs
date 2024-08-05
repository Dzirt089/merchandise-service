using MediatR;

namespace OzonEdu.MerchandiseService.Domain.Models
{
	/// <summary>
	/// Сущности с изменяемыми свойствами и с уникальным ID, сравнение происходит по ID.
	/// Частично шаблон взят отсюда
	/// https://learn.microsoft.com/ru-ru/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/domain-events-design-implementation
	/// Entity может в себе содержать св-ва других ValueObject, так и св-ва Entity
	/// </summary>
	public abstract class Entity
	{
		protected int? _requestedHashCode;
		public virtual long Id { get; protected set; }
		private readonly List<INotification> _domainEvents = new();
		public IReadOnlyCollection<INotification> DomainEvents => _domainEvents.AsReadOnly();
		public void AddDomainEvent(INotification eventItem)
		{
			_domainEvents.Add(eventItem);
		}
		public void RemoveDomainEvent(INotification eventItem)
		{
			_domainEvents.Remove(eventItem);
		}
		public void ClearDomainEvents()
		{
			_domainEvents.Clear();
		}
		public bool IsTransient()
		{
			return Id == default(long);
		}

		public override bool Equals(object? obj)
		{
			if (obj is not Entity entity) return false;
			if (ReferenceEquals(this, entity)) return true;
			if (GetType() != entity.GetType()) return false;
			if (entity.IsTransient() || IsTransient()) return false;
			else return entity.Id == Id;
		}

		public override int GetHashCode()
		{
			if (!IsTransient())
			{
				if (!_requestedHashCode.HasValue) _requestedHashCode = HashCode.Combine(Id, 31);
				return _requestedHashCode.Value;
			}
			else return base.GetHashCode();
		}

		public static bool operator ==(Entity left, Entity right)
		{
			if (Equals(left, null)) return Equals(right, null) ? true : false;
			else return left.Equals(right);
		}

		public static bool operator !=(Entity left, Entity right)
		{
			return !(left == right);
		}
	}
}
