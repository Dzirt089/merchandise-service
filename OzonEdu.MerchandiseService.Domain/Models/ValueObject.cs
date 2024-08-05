namespace OzonEdu.MerchandiseService.Domain.Models
{
	/// <summary>
	/// Объект значений. Нет уникального ID (удостоверения) и они НЕИЗМЕНЯЕМЫЕ. Сравнение по значению
	/// Шаблон взят из https://learn.microsoft.com/ru-ru/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/implement-value-objects
	/// Объект - это не сущность чего либо с уникальным ID и другими (изменяемыми) св-ми. 
	/// Например адрес - это объект значения.
	/// ValueObject может в себе содержать св-ва других ValueObject, но не может содержать Entity
	/// </summary>
	public abstract class ValueObject
	{
		protected static bool EqualOperator(ValueObject left, ValueObject right)
		{
			if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
			{
				return false;
			}
			return ReferenceEquals(left, null) || left.Equals(right);
		}

		protected static bool NotEqualOperator(ValueObject left, ValueObject right)
		{
			return !EqualOperator(left, right);
		}

		protected abstract IEnumerable<object> GetEqualityComponents();

		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}

			var other = (ValueObject)obj;

			return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
		}

		public override int GetHashCode()
		{
			return GetEqualityComponents()
				.Select(x => x != null ? x.GetHashCode() : 0)
				.Aggregate((x, y) => x ^ y);
		}

		public ValueObject GetCopy()
		{
			return MemberwiseClone() as ValueObject;
		}
	}
}
