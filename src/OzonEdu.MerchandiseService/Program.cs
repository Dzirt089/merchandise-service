using MediatR;

using OzonEdu.MerchandiseService.Application.Behaviors;
using OzonEdu.MerchandiseService.Application.Handlers;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;
using OzonEdu.MerchandiseService.GrpcServices;
using OzonEdu.MerchandiseService.Infrastructure;
using OzonEdu.MerchandiseService.Infrastructure.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Repositories.Implementation;
using OzonEdu.MerchandiseService.Services;
public class Program
{
	private static void Main(string[] args)
	{
		var builder = WebApplication.CreateBuilder(args);
		var configuration = builder.Configuration;

		builder.Services.AddMediatR(cfg =>
			cfg.RegisterServicesFromAssembly(typeof(GetRequestsByEmployeeQueryHandler).Assembly));

		builder.Services.AddTransient(
			typeof(IPipelineBehavior<,>),
			typeof(DomainEventPublishingBehavior<,>));

		builder.Services.AddMerchandiseServicesEntityFrameworkDb(configuration);

		builder.AddInfrastructureLogger();
		builder.AddInfrastructureOpenTelemetry();
		builder.Services.AddEndpointsApiExplorer();
		builder.ConfigurePorts();
		builder.Services.AddInfrastructureSwagger();
		builder.Services.AddInfrastructureEndpoints();
		builder.Services.AddInfrastructureMiddlewareGrpc();
		builder.Services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

		builder.Services.AddSingleton<IMerchService, MerchService>();

		builder.Services.AddScoped<ISkuPresetRepository, SkuPresetRepository>();
		builder.Services.AddScoped<IMerchandiseRepository, MerchandiseRepository>();

		var app = builder.Build();

		// Подключаем миддлеваре библиотеки
		app.AddInfrastructureMiddlewareHttp();
		// app.UseHttpsRedirection();

		app.UseAuthorization();

		app.MapGrpcService<MerchApiGrpsService>();
		app.Run();
	}
}
