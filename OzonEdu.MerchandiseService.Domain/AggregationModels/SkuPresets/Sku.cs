using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets
{
	/// <summary>
	/// SKU - Stock Keeping Unit - это уникальный идентификатор товара, который используется для отслеживания товара в инвентаре.
	/// </summary>
	public class Sku : ValueObject
	{
		/// <summary>
		/// Конструктор
		/// </summary>
		/// <param name="value"></param>
		private Sku(long value)
		{
			Value = value;
		}

		/// <summary>
		/// Значение
		/// </summary>
		public long Value { get; }

		/// <summary>
		/// Создать SKU
		/// </summary>
		/// <param name="value"></param>
		/// <returns></returns>
		/// <exception cref="DomainException"></exception>
		public static Sku Create(long value)
		{
			//TODO: Здесь можно добавить всю необходимую валидацию, если надо
			if (value <= 0)
			{
				throw new DomainException("Sku value must be greater than 0");
			}
			return new Sku(value);
		}

		/// <summary>
		/// Получить компоненты равенства
		/// </summary>
		/// <returns></returns>
		protected override IEnumerable<object> GetEqualityComponents()
		{
			yield return Value;
		}
	}
}
