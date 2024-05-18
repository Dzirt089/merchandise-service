namespace OzonEdu.MerchandiseService.Services
{
    public interface IMerchService
    {
        Task<List<MerchItem>> GetAll(CancellationToken token);
        Task<MerchItem> GetById(long id, CancellationToken token);
    }
}
