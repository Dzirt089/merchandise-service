using Microsoft.EntityFrameworkCore;

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
			var result = await _context.MerchandiseRequests.AddAsync(merchandiseRequest, cancellationToken);
			return result.Entity.Id;
		}

		public Task<IReadOnlyCollection<MerchandiseRequest>> GetAllProcessingRequestsAsync(CancellationToken cancellationToken)
		{
			throw new NotImplementedException();
		}

		public async Task<IReadOnlyCollection<MerchandiseRequest>> GetByEmployeeEmailAsync(Email email, CancellationToken cancellationToken)
		{
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
			_context.MerchandiseRequests.Update(merchandiseRequest);
		}
	}
}
