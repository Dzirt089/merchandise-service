using OzonEdu.MerchandiseService.Infrastructure.Configuration;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using OzonEdu.MerchandiseService.Services;
public class Program
{
    private static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);
        
        builder.Services.AddEndpointsApiExplorer();
        
        //���������� ������� ���������� ����� �� ����� ������������� (�������, �����, �������, ����������).        
        builder.Services.AddInfrastructure();
        builder.Services.AddSingleton<IMerchService, MerchService>();
        builder.Services.AddGrpc();

        var app = builder.Build();

        //���������� ���������� ����������
        app.AddInfrastructureMiddleware();
        app.UseHttpsRedirection();

        app.UseAuthorization();

        app.MapGrpcService<MerchApiGrpsService>();
        app.MapControllers();
        app.Run();
    }
}