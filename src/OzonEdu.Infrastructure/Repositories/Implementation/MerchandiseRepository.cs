using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;

using System.Diagnostics;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class MerchandiseRepository : IMerchandiseRepository
	{
		private readonly MerchandiseDbContext _context;
		private readonly ActivitySource _activitySource;

		public MerchandiseRepository(MerchandiseDbContext context, ActivitySource activitySource = null)
		{
			_context = context;
			_activitySource = activitySource ?? MerchandiseTelemetry.ActivitySource;
		}

		public async Task<long> CreateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.CreateAsync", ActivityKind.Internal);

			var result = await _context.MerchandiseRequests.AddAsync(merchandiseRequest, cancellationToken);
			return result.Entity.Id;
		}

		public async Task<IReadOnlyCollection<MerchandiseRequest>> GetAllProcessingRequestsAsync(CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.GetAllProcessingRequestsAsync", ActivityKind.Internal);

			return await _context.MerchandiseRequests
				.Where(x => x.Status == MerchandiseRequestStatus.Processing)
				.Include(x => x.SkuPreset)
				.ThenInclude(x => x.SkuCollection)
				.ToListAsync(cancellationToken);
		}

		public async Task<IReadOnlyCollection<MerchandiseRequest>> GetByEmployeeEmailAsync(Email email, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.GetByEmployeeEmailAsync", ActivityKind.Internal);

			return await _context.MerchandiseRequests
				.Where(x => x.Employee.Email == email)
				.Include(x => x.SkuPreset)
				.ThenInclude(x => x.SkuCollection)
				.ToListAsync(cancellationToken);
		}

		public async Task<MerchandiseRequest> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.GetByIdAsync", ActivityKind.Internal);

			var result = await _context.MerchandiseRequests
				.Include(x => x.SkuPreset)
				.ThenInclude(x => x.SkuCollection)
				.SingleOrDefaultAsync(x => x.Id == id, cancellationToken);

			return result ?? throw new InvalidOperationException($"Merchandise request with id {id} was not found.");
		}

		public Task UpdateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.UpdateAsync", ActivityKind.Internal);

			var entry = _context.Entry(merchandiseRequest);
			if (entry.State == EntityState.Detached)
			{
				if (merchandiseRequest.Id <= 0)
				{
					return Task.CompletedTask;
				}

				_context.MerchandiseRequests.Attach(merchandiseRequest);
				entry = _context.Entry(merchandiseRequest);
			}

			if (entry.State == EntityState.Unchanged)
			{
				entry.State = EntityState.Modified;
			}

			return Task.CompletedTask;
		}
	}
}
