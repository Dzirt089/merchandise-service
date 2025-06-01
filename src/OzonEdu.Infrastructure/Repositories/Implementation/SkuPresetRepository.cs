using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class SkuPresetRepository : ISkuPresetRepository
	{
		private readonly MerchandiseDbContext _context;

		public SkuPresetRepository(MerchandiseDbContext context)
		{
			_context = context;
		}

		public async Task<SkuPreset> FindByTypeAsync(PresetType type, ClothingSize clothingSize, CancellationToken cancellationToken)
		{
			var result = await _context.Skus
				.Where(x => x.PresetTypeId == type.Id
				&& (x.ClothingSize == null
					|| x.ClothingSize == clothingSize.Id))
				.ToListAsync(cancellationToken);

			if (!result.Any()) throw new InvalidOperationException(
			$"Для пресета '{type.Name}' и размера '{clothingSize.Name}' ничего не найдено.");

			var skus = result.Select(x => Sku.Create(x.Id)).ToList();

			var preset = new SkuPreset(
				id: 0,
				skus: skus,
				type: type);
			return preset;
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
