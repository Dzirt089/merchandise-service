using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;

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
			_activitySource = activitySource;
		}

		public async Task<long> CreateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.CreateAsync", ActivityKind.Internal);

			var result = await _context.MerchandiseRequests.AddAsync(merchandiseRequest, cancellationToken);
			return result.Entity.Id;
		}

		public Task<IReadOnlyCollection<MerchandiseRequest>> GetAllProcessingRequestsAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<IReadOnlyCollection<MerchandiseRequest>> GetByEmployeeEmailAsync(Email email, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.GetByEmployeeEmailAsync", ActivityKind.Internal);

			var result = await _context.MerchandiseRequests
				.Where(x => x.Employee.Email == email)
				.ToListAsync(cancellationToken);

			return result;
		}

		public Task<MerchandiseRequest> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task UpdateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			using var activity = _activitySource.StartActivity("MerchandiseRepository.UpdateAsync", ActivityKind.Internal);

			_context.MerchandiseRequests.Update(merchandiseRequest);
		}
	}
}
