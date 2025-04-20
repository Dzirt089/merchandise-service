namespace OzonEdu.MerchandiseService.Http.Services
{
	public interface IHttpService
	{
		Task<List<ItemPosition>> GetAll(CancellationToken token);
		Task<ItemPosition> GetById(long id, CancellationToken token);
	}
}
