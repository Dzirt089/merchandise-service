using OzonEdu.MerchandiseService.Infrastructure.Configuration;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using OzonEdu.MerchandiseService.Services;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        //������ �����������, ����� ����������� ������� ������ � ��
        IConfigurationRoot _configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .Build();


        builder.Services.AddEndpointsApiExplorer();
        
        //���������� ���������� ����� �� ����� ������������� (�������, �����, �������, ����������). ����� �� �������� �������� ������.
        builder.Services.AddInfrastructure(_configuration);
        builder.Services.AddSingleton<IMerchService, MerchService>();

        var app = builder.Build();

        //���������� ���������� �� ���������� "RequestResponseLoggingMiddleware", �������� ���.
        //���� �� ����������, ��� � ���������� "Infrastructure" ����������, ����� ��������.
        app.UseMiddleware<RequestResponseLoggingMiddleware>();
        app.UseHttpsRedirection();

        app.UseAuthorization();
        app.MapControllers();
        app.Run();
    }
}