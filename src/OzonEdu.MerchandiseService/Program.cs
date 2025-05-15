using OzonEdu.MerchandiseService.GrpcServices;
using OzonEdu.MerchandiseService.Infrastructure;
using OzonEdu.MerchandiseService.Infrastructure.Filters;
using OzonEdu.MerchandiseService.Services;
public class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);

		builder.AddInfrastructureLogger();
		builder.AddInfrastructureOpenTelemetry();
		builder.Services.AddEndpointsApiExplorer();
		builder.ConfigurePorts();
		builder.Services.AddInfrastructureSwagger();
		builder.Services.AddInfrastructureEndpoints();
		builder.Services.AddInfrastructureMiddlewareGrpc();
		builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

		builder.Services.AddSingleton<IMerchService, MerchService>();

		var app = builder.Build();

		// Подключаем миддлеваре библиотеки
		app.AddInfrastructureMiddlewareHttp();
		// app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapGrpcService<MerchApiGrpsService>();
		app.Run();
	}
}
