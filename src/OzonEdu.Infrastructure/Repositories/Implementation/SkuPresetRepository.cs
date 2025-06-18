using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class SkuPresetRepository : ISkuPresetRepository
	{
		private readonly MerchandiseDbContext _context;
		private readonly ActivitySource _activitySource;

		public SkuPresetRepository(MerchandiseDbContext context, ActivitySource activitySource = null)
		{
			_context = context;
			_activitySource = activitySource;
		}

		public async Task<SkuPreset> FindByTypeAsync(PresetType type, ClothingSize clothingSize, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("SkuPresetRepository.FindByTypeAsync", ActivityKind.Internal);

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

		public async Task CreateAsync(SkuPreset skuPreset, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("SkuPresetRepository.CreateAsync", ActivityKind.Internal);

			await _context.SkuPresets.AddAsync(skuPreset, cancellationToken);
		}
	}
}
