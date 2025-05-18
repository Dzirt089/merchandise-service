using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class SkuPresetRepository : ISkuPresetRepository
	{
		public Task<SkuPreset> FindByTypeAsync(PresetType type, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
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
