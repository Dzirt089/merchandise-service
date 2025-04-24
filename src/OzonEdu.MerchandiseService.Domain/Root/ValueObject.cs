namespace OzonEdu.MerchandiseService.Domain.Root
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
		/// <summary>
		/// Оператор равенства
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		protected static bool EqualOperator(ValueObject left, ValueObject right)
		{
			if (ReferenceEquals(left, null) ^ ReferenceEquals(right, null))
			{
				return false;
			}
			return ReferenceEquals(left, null) || left.Equals(right);
		}

		/// <summary>
		/// Оператор неравенства
		/// </summary>
		/// <param name="left"></param>
		/// <param name="right"></param>
		/// <returns></returns>
		protected static bool NotEqualOperator(ValueObject left, ValueObject right)
		{
			return !EqualOperator(left, right);
		}

		/// <summary>
		/// Получить компоненты равенства
		/// </summary>
		/// <returns></returns>
		protected abstract IEnumerable<object> GetEqualityComponents();

		/// <summary>
		/// Сравнение объектов
		/// </summary>
		/// <param name="obj"></param>
		/// <returns></returns>
		public override bool Equals(object obj)
		{
			if (obj == null || obj.GetType() != GetType())
			{
				return false;
			}

			var other = (ValueObject)obj;

			return GetEqualityComponents().SequenceEqual(other.GetEqualityComponents());
		}

		/// <summary>
		/// Получение хэш-кода
		/// </summary>
		/// <returns></returns>
		public override int GetHashCode()
		{
			return GetEqualityComponents()
				.Select(x => x != null ? x.GetHashCode() : 0)
				.Aggregate((x, y) => x ^ y);
		}

		/// <summary>
		/// Получить копию объекта
		/// </summary>
		/// <returns></returns>
		public ValueObject GetCopy()
		{
			return MemberwiseClone() as ValueObject;
		}
	}
}
