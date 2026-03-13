using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;

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
			_activitySource = activitySource ?? MerchandiseTelemetry.ActivitySource;
		}

		public async Task<SkuPreset> FindByTypeAsync(PresetType type, ClothingSize clothingSize, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("SkuPresetRepository.FindByTypeAsync", ActivityKind.Internal);

			var availableSkuIds = await _context.Skus
				.Where(x => x.PresetTypeId == type.Id
					&& (x.ClothingSize == null || x.ClothingSize == clothingSize.Id))
				.Select(x => x.Id)
				.ToListAsync(cancellationToken);

			if (!availableSkuIds.Any())
			{
				throw new InvalidOperationException($"Для пресета '{type.Name}' и размера '{clothingSize.Name}' ничего не найдено.");
			}

			var presets = await _context.SkuPresets
				.Include(x => x.SkuCollection)
				.Where(x => x.Type == type)
				.ToListAsync(cancellationToken);

			var preset = presets.FirstOrDefault(x => x.SkuCollection.All(sku => availableSkuIds.Contains(sku.Value)));

			return preset ?? throw new InvalidOperationException(
				$"Для пресета '{type.Name}' и размера '{clothingSize.Name}' ничего не найдено.");
		}

		public async Task<SkuPreset> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("SkuPresetRepository.GetByIdAsync", ActivityKind.Internal);

			var result = await _context.SkuPresets
				.Include(x => x.SkuCollection)
				.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

			return result ?? throw new InvalidOperationException($"Sku preset with id {id} was not found.");
		}

		public async Task CreateAsync(SkuPreset skuPreset, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("SkuPresetRepository.CreateAsync", ActivityKind.Internal);

			await _context.SkuPresets.AddAsync(skuPreset, cancellationToken);
		}
	}
}
