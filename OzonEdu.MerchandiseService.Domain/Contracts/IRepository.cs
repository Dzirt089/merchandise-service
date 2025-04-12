namespace OzonEdu.MerchandiseService.Domain.Contracts
{
	public interface IRepository<TAggregationRoot>
	{
		IUnitOfWork UnitOfWork { get; }
	}
}
