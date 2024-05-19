namespace OzonEdu.MerchandiseService.Http.Services
{
    public interface IHttpService
    {
        Task<List<HttpItem>> GetAll(CancellationToken token);
        Task<HttpItem> GetById(long id, CancellationToken token);
    }
}
