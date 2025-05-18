using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;

namespace OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation
{
	public class MerchandiseRepository : IMerchandiseRepository
	{
		private readonly MerchandiseDbContext _context;

		public MerchandiseRepository(MerchandiseDbContext context)
		{
			_context = context;
		}

		public async Task<long> CreateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			await _context.MerchandiseRequests.AddAsync(merchandiseRequest, cancellationToken);
			return merchandiseRequest.Id;
		}

		public Task<IReadOnlyCollection<MerchandiseRequest>> GetAllProcessingRequestsAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<IReadOnlyCollection<MerchandiseRequest>> GetByEmployeeEmailAsync(Email email, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task<MerchandiseRequest> GetByIdAsync(long id, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public Task UpdateAsync(MerchandiseRequest merchandiseRequest, CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}
	}
}
