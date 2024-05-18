using OzonEdu.MerchandiseService.Infrastructure.Configuration;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using OzonEdu.MerchandiseService.Services;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        builder.Services.AddEndpointsApiExplorer();

        builder.Services.AddInfrastructure();
        builder.Services.AddSingleton<IMerchService, MerchService>();

        var app = builder.Build();

        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}