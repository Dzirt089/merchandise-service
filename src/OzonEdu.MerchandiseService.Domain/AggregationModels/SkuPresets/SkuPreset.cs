using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets
{
	/// <summary>
	/// Идентификатор набора мерча
	/// </summary>
	public sealed class SkuPreset : Entity
	{

		/// <summary>
		/// Представляет собой предварительно определенный набор артикулов (единиц хранения на складе), связанных с определенным типом.
		/// </summary>
		/// <param name="id">Уникальный идентификатор для предустановленного артикула.</param>
		/// <param name="skus">Набор артикулов, включенных в предустановку, доступный только для чтения. Не может быть пустым.</param>
		/// <param name="type">Тип предустановки, указывающий на ее категорию или назначение.</param>
		public SkuPreset(long id, IReadOnlyCollection<Sku> skus, PresetType type)
		{
			Id = id;
			SkuCollection.AddRange(skus); // Заполняем приватное поле
			Type = type;
		}

		// Конструктор для EF Core
		private SkuPreset() { }

		/// <summary>
		/// Набор артикулов
		/// </summary>
		// Публичное свойство только для чтения
		public List<Sku> SkuCollection { get; private set; } = new();

		/// <summary>
		/// Тип набора мерча
		/// </summary>
		public PresetType Type { get; }

		/// <summary>
		/// Добавить артикулы в набор
		/// </summary>
		/// <param name="skus"></param>
		/// <exception cref="DomainException"></exception>
		public void AddToPreset(IReadOnlyCollection<Sku> skus)
		{
			var doubles = SkuCollection
				.Select(x => x.Value)
				.Intersect(skus
					.Select(x => x.Value)
					.ToList()).ToList();

			if (doubles.Count > 0)
			{
				throw new DomainException($"Sku with ids {string.Join(",", doubles)} already added");
			}

			SkuCollection.AddRange(skus); // Добавляем новые артикулы в коллекцию
		}

		/// <summary>
		/// Удаляет указанные артикулы из текущей предустановки.
		/// </summary>
		/// <remarks>Этот метод гарантирует, что все артикулы из предоставленной коллекции присутствуют в текущей предустановке
		/// перед попыткой их удаления. Если какой-либо артикул отсутствует, операция прерывается и генерируется  <see
		/// cref="DomainException"/> is thrown.</remarks>
		/// <param name="skus">Коллекция SKU для удаления. Каждый SKU должен присутствовать в текущей предустановке.</param>
		/// <exception cref="DomainException">Возникает, если один или несколько SKU из предоставленной коллекции отсутствуют в текущей предустановке.</exception>
		public void DeleteFromPreset(IReadOnlyCollection<Sku> skus)
		{
			var missing = SkuCollection
				.Select(x => x.Value)
				.Except(skus.
					Select(x => x.Value)
					.ToList()).ToList();

			if (missing.Count > 0)
			{
				throw new DomainException($"Sku with ids {string.Join(",", missing)} not exists");
			}

			foreach (var sku in skus)
			{
				SkuCollection.Remove(sku);
			}
		}

		//public static IEnumerable<PresetType> GetAll()
		//{
		//	return [PresetType.WelcomePack, PresetType.ConferenceListenerPack, PresetType.ConferenceSpeakerPack, PresetType.ProbationPeriodEndingPack, PresetType.VeteranPack];
		//}
	}
}
