using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

using OzonEdu.MerchandiseService.Infrastructure.Interceptors;
using OzonEdu.MerchandiseService.Infrastructure.Middlewares;
using OzonEdu.MerchandiseService.Infrastructure.StartapFilters;

using System.Net;
using System.Reflection;

namespace OzonEdu.MerchandiseService.Infrastructure
{
	public static class HostBuilderExtensions
	{
		//Создаю кофигурацию, чтобы библиотечке считать данные с неё
		static IConfigurationRoot? _configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.Build();

		/// <summary>
		/// Подключаем Swagger сервис
		/// </summary>
		public static IServiceCollection AddInfrastructureSwagger(this IServiceCollection services)
		{
			services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();

			services.AddSwaggerGen(options =>
			{
				options.SwaggerDoc("v1", new OpenApiInfo { Title = "OzonEdu.MerchandiseService", Version = "v1" });
				options.CustomSchemaIds(x => x.FullName);
				var xmlFileName = Assembly.GetExecutingAssembly().GetName().Name + ".xml";
				var xmlFilePath = Path.Combine(AppContext.BaseDirectory, xmlFileName);
				options.IncludeXmlComments(xmlFilePath);
			});

			services.AddSingleton<IStartupFilter, TerminalStartupFilter>();


			return services;
		}

		public static IServiceCollection AddInfrastructureEndpoints(this IServiceCollection services)
		{
			services.AddSingleton<IStartupFilter, TerminalStartupFilter>();
			return services;
		}

		public static IServiceCollection AddInfrastructureLogger(this IServiceCollection services)
		{
			//Добавляю свои настройки для логов 
			if (_configuration != null)
			{
				services.AddLogging(builder =>
				{
					builder.ClearProviders();
					builder.AddConfiguration(_configuration.GetSection("Logging"));
					builder.AddConsole();
					builder.AddFile(@"Logs/app-{Date}.txt");
				});
			}
			return services;
		}

		/// <summary>
		/// Подключаем для логгирования Http запросов и ответов, роутинг, конечные точки контроллеров.   
		/// </summary>
		public static IApplicationBuilder AddInfrastructureMiddlewareHttp(this IApplicationBuilder app)
		{
			app.UseMiddleware<RequestLoggingMiddleware>();
			app.UseMiddleware<ResponseLoggingMiddleware>();
			app.UseRouting();
			app.UseEndpoints(endpoints => endpoints.MapControllers());
			return app;
		}

		/// <summary>
		/// Подключаем для логгирования Grpc запросов.
		/// </summary>
		public static IServiceCollection AddInfrastructureMiddlewareGrpc(this IServiceCollection services)
		{
			services.AddGrpc(services => services.Interceptors.Add<LoggingInterceptor>());
			return services;
		}

		public static WebApplicationBuilder ConfigurePorts(this WebApplicationBuilder builder)
		{
			var httpPortEnv = Environment.GetEnvironmentVariable("HTTP_PORT");
			if (!int.TryParse(httpPortEnv, out var httpPort))
				httpPort = 5000;

			var grpcPortEnv = Environment.GetEnvironmentVariable("GRPC_PORT");
			if (!int.TryParse(grpcPortEnv, out var grpcPort))
				grpcPort = 5002;

			builder.WebHost.ConfigureKestrel(options =>
			{
				Listen(options, httpPort, HttpProtocols.Http1);
				Listen(options, grpcPort, HttpProtocols.Http2);
			});

			return builder;
		}
		static void Listen(KestrelServerOptions kestrelServerOptions, int? port, HttpProtocols protocols)
		{
			if (port == null)
				return;

			var address = IPAddress.Any;


			kestrelServerOptions.Listen(address, port.Value, listenOptions => { listenOptions.Protocols = protocols; });
		}
	}
}
