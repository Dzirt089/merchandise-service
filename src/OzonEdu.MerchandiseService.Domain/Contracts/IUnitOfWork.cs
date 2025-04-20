namespace OzonEdu.MerchandiseService.Domain.Contracts
{
	public interface IUnitOfWork
	{
		Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);

		/// <summary>
		/// Комитим все изменения
		/// </summary>
		/// <param name="cancellationToken"></param>
		/// <returns></returns>
		Task<int> SaveEntitiesAsync(CancellationToken cancellationToken = default);
	}
}
