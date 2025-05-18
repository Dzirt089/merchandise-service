namespace OzonEdu.MerchandiseService.Domain.Contracts
{
	public interface IUnitOfWork
	{
		/// <summary>
		/// Initiates a transaction encompassing all actions (Repositories, Events).
		/// </summary>
		ValueTask StartTransactionAsync(CancellationToken cancellationToken);

		/// <summary>
		/// Commits all changes made during the transaction.
		/// </summary>
		/// <param name="cancellationToken">Token to monitor for cancellation requests.</param>
		/// <returns>A task that represents the asynchronous save operation.</returns>
		Task SaveChangesAsync(CancellationToken cancellationToken);
	}
}
