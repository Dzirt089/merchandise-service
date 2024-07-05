using System.Text.Json.Serialization;

namespace OzonEdu.MerchandiseService.Http.Services
{
	/// <summary>
	/// Модель мерча на стороне клиента. 
	/// </summary>
	public class ItemPosition
	{
		public ItemPosition(long id, string itemName, int quantity)
		{
			IdPosition = id;
			ItemNamePosition = itemName;
			QuantityPosition = quantity;
		}
		public ItemPosition() { }

		/// <summary>
		/// Идентификатор мерча
		/// </summary>
		[JsonPropertyName("IdPosition")]
		public long IdPosition { get; set; }


		/// <summary>
		/// Наименования меча
		/// </summary>
		[JsonPropertyName("ItemNamePosition")]
		public string ItemNamePosition { get; set; }

		/// <summary>
		/// Остаток на складе позиции мерча
		/// </summary>
		[JsonPropertyName("QuantityPosition")]
		public int QuantityPosition { get; set; }

		public override string ToString() => $"{IdPosition}, {ItemNamePosition}, {QuantityPosition}";
	}
}
