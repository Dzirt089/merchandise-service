using OzonEdu.MerchandiseService.Http.Services;

class Program
{
    private static IHttpService _service = new HttpService(new HttpClient());

    static async Task Main(string[] args)
    {
        List<HttpItem> items = await _service.GetAll(CancellationToken.None);
        HttpItem item = await _service.GetById(2, CancellationToken.None);
    }
}