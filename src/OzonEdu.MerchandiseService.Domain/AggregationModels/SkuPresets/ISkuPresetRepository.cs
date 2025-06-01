using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.Contracts;

namespace OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets
{
	public interface ISkuPresetRepository : IRepository<SkuPreset>
	{
		/// <summary>
		/// Сохранение набора мерча
		/// </summary>
		/// <param name="skuPreset"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task SaveAsync(SkuPreset skuPreset, CancellationToken cancellationToken);

		/// <summary>
		/// Получение набора мерча по идентификатору
		/// </summary>
		/// <param name="id"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<SkuPreset> GetByIdAsync(long id, CancellationToken cancellationToken);

		/// <summary>
		/// Ищем набор мерча по типу
		/// </summary>
		/// <param name="type"></param>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<SkuPreset> FindByTypeAsync(PresetType type, ClothingSize clothingSize, CancellationToken cancellationToken);
	}
}
