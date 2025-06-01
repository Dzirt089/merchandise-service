using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests
{
	/// <summary>
	/// Размер одежды
	/// </summary>
	public class ClothingSize : Enumeration
	{
		public static ClothingSize XS = new(1, nameof(XS), "Extra smail");
		public static ClothingSize S = new(2, nameof(S), "Smail");
		public static ClothingSize M = new(3, nameof(M), "Medium");
		public static ClothingSize L = new(4, nameof(L), "Large");
		public static ClothingSize XL = new(5, nameof(XL), "Extra large");
		public static ClothingSize XXL = new(6, nameof(XXL), "Extra extra large");

		/// <summary>
		/// Описание размера одежды
		/// </summary>
		public string Description { get; }

		private ClothingSize() { }

		/// <summary>
		/// Конструктор ClothingSize (размер одежды)
		/// </summary>
		/// <param name="id"></param>
		/// <param name="name"></param>
		/// <param name="description"></param>
		public ClothingSize(int id, string name, string description) : base(id, name)
			=> Description = description;



		/// <summary>
		/// Метод парсинга строки в ClothingSize (размер одежды)
		/// </summary>
		/// <param name="size"></param>
		/// <returns></returns>
		/// <exception cref="DomainException"></exception>
		public static ClothingSize Parse(string size) =>
			size?.ToUpper() switch
			{
				"XS" => XS,
				"S" => S,
				"M" => M,
				"L" => L,
				"XL" => XL,
				"XXL" => XXL,
				_ => throw new DomainException($"Unknown size: {size}")
			};
	}
}
