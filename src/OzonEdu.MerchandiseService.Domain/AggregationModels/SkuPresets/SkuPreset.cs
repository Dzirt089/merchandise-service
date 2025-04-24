using OzonEdu.MerchandiseService.Domain.Root;
using OzonEdu.MerchandiseService.Domain.Root.Exceptions;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets
{
	/// <summary>
	/// Идентификатор набора мерча
	/// </summary>
	public sealed class SkuPreset : Entity
	{
		public SkuPreset(long id, IReadOnlyCollection<Sku> skus, PresetType type)
		{
			Id = id;
			SkuCollection = skus;
			Type = type;
		}

		/// <summary>
		/// Набор артикулов
		/// </summary>
		public IReadOnlyCollection<Sku> SkuCollection { get; set; }

		/// <summary>
		/// Тип набора мерча
		/// </summary>
		public PresetType Type { get; }

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

			SkuCollection = SkuCollection.Union(skus).ToList();
		}

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

			SkuCollection = SkuCollection.Except(skus).ToList();
		}
	}
}
