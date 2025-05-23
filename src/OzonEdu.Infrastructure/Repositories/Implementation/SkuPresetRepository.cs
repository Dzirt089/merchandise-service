using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class SkuPresetRepository : ISkuPresetRepository
	{
		public async Task<SkuPreset> FindByTypeAsync(PresetType type, CancellationToken cancellationToken)
		{
			// Здесь должна быть логика для поиска SkuPreset по типу
			// Например, обращение к базе данных или другому источнику данных
			// Для примера, возвращаем null, если не найден
			return await Task.FromResult<SkuPreset>(null);
		}

		public Task<SkuPreset> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task SaveAsync(SkuPreset skuPreset, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
