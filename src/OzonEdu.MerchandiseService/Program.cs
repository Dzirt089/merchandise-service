using OzonEdu.MerchandiseService.Infrastructure.Configuration;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using OzonEdu.MerchandiseService.Services;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        //Создаю кофигурацию, чтобы библиотечке считать данные с неё
        IConfigurationRoot _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();


        builder.Services.AddEndpointsApiExplorer();
        
        //Подключаем библиотеку почти со всеми подключениями (Сваггер, логги, фильтры, контроллер). Чтобы не засорять основной проект.
        builder.Services.AddInfrastructure(_configuration);
        builder.Services.AddSingleton<IMerchService, MerchService>();

        var app = builder.Build();

        //Подключаем Миддлеваре из библиотеки "RequestResponseLoggingMiddleware", напрямую тут.
        //Пока не разобрался, как в библиотеке "Infrastructure" подключить, чтобы работало.
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}