using Grpc.Net.Client;

using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.OpenApi.Models;

using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

using OzonEdu.MerchandiseService.Application.Contracts;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Configurations;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Infrastructure.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Interceptors;
using OzonEdu.MerchandiseService.Infrastructure.StartapFilters;
using OzonEdu.StockApi.Grpc;

using Serilog;
using Serilog.Events;

using System.Diagnostics;
using System.Net;
using System.Reflection;


namespace OzonEdu.MerchandiseService.Infrastructure
{
	public static class HostBuilderExtensions
	{
		public static IServiceCollection AddStockGrpcServiceClient(this IServiceCollection services, IConfiguration configuration)
		{
			var connectionAddres = configuration
				.GetSection(nameof(StockApiGrpcServiceConfiguration))
				.Get<StockApiGrpcServiceConfiguration>()
				.ServerAddress;

			if (string.IsNullOrWhiteSpace(connectionAddres))
			{
				connectionAddres = configuration
					.Get<StockApiGrpcServiceConfiguration>()
					.ServerAddress;
			}

			services.AddScoped<StockApiGrpc.StockApiGrpcClient>(provider =>
			{
				var channel = GrpcChannel.ForAddress(connectionAddres);
				return new StockApiGrpc.StockApiGrpcClient(channel);
			});

			return services;
		}

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

		/// <summary>
		/// Подключаем сервис с конечными точками (/ready; /live; /version)
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddInfrastructureEndpoints(this IServiceCollection services)
		{
			services.AddSingleton<IStartupFilter, TerminalStartupFilter>();
			return services;
		}

		/// <summary>
		/// Подключаем Serilog для логгирования приложения.
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static WebApplicationBuilder AddInfrastructureLogger(this WebApplicationBuilder app)
		{
			// Создаем папку для логов, если её нет
			var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
			Directory.CreateDirectory(logPath);

			app.Host.UseSerilog(
				(context, services, config) =>
				{
					// Базовая конфигурация из appsettings.json
					config.ReadFrom.Configuration(context.Configuration);

					// Кастомные настройки, которые сложно выразить через JSON
					config.Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
						  .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

					// Кастомные приемники, которые сложно описать в JSON
					if (context.HostingEnvironment.IsDevelopment())
					{
						config.WriteTo.Debug();
					}
				});

			return app;
		}

		/// <summary>
		/// Добавляем OpenTelemetry для трассировки и экспорта в Jaeger.
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static WebApplicationBuilder AddInfrastructureOpenTelemetry(this WebApplicationBuilder app)
		{
			app.Services.AddOpenTelemetry()
			.ConfigureResource(res => res
				.AddService(
					serviceName: app.Environment.ApplicationName,
					serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version",
					serviceInstanceId: Environment.MachineName))
			.WithTracing(tb =>
			{
				tb
					.AddAspNetCoreInstrumentation(opt =>
						opt.Filter = x =>
							!x.Request.Path.StartsWithSegments("/health"))    // Автоматически работает с HTTP и gRPC сервером

					.AddHttpClientInstrumentation()
					.AddGrpcClientInstrumentation()  // Для gRPC клиента
					.AddEntityFrameworkCoreInstrumentation() // Для EF Core
					.AddSqlClientInstrumentation()    // Для Dapper (и любого ADO.NET провайдера)
													  //.AddSource("OzonEdu.MerchandiseService")
					.AddConsoleExporter()
					.AddJaegerExporter(o =>
					{
						// Считаем из переменных окружения, или берём дефолтные
						o.AgentHost = Environment.GetEnvironmentVariable("OTEL_EXPORTER_JAEGER_AGENT_HOST") ?? "jaeger";
						o.AgentPort = int.Parse(Environment.GetEnvironmentVariable("OTEL_EXPORTER_JAEGER_AGENT_PORT") ?? "6831");
						var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_JAEGER_ENDPOINT");
						if (!string.IsNullOrEmpty(endpoint))
							o.Endpoint = new Uri(endpoint);
						o.Protocol = JaegerExportProtocol.HttpBinaryThrift;

						//o.AgentHost = "localhost"; // адрес Jaeger-агента
						//o.AgentPort = 6831;        // порт Jaeger-агента (UDP)
						//o.Protocol = JaegerExportProtocol.HttpBinaryThrift;
						//o.Endpoint = new Uri("http://localhost:14268/api/traces"); // порт HTTP-коллектора Jaeger
					});
			});

			var activitySource = new ActivitySource("OzonEdu.MerchandiseService");
			using var activity = activitySource.StartActivity("ManualTestSpan");
			activity?.SetTag("custom.tag", "hello_jaeger");
			activity?.SetStatus(ActivityStatusCode.Ok);

			return app;
		}

		/// <summary>
		/// Подключаем для логгирования Http запросов и ответов, роутинг, конечные точки контроллеров.   
		/// </summary>
		public static IApplicationBuilder AddInfrastructureMiddlewareHttp(this IApplicationBuilder app)
		{
			app.UseSerilogRequestLogging(opts =>
			{
				// Опционально: кастомизируем шаблон сообщения
				opts.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

				opts.GetLevel = (httpContext, elapsed, ex) =>
				{
					if (ex != null || httpContext.Response.StatusCode >= 500)
						return LogEventLevel.Error;
					if (httpContext.Response.StatusCode >= 400)
						return LogEventLevel.Warning;
					return LogEventLevel.Information;
				};

				opts.EnrichDiagnosticContext = (IDiagnosticContext diagnosticContext, HttpContext httpContext) =>
				{
					// **Вот здесь** пытаемся взять детали ошибки, если они туда были положены
					if (httpContext.Items.TryGetValue("ErrorDetails", out var ed) && ed is GlobalErrorDetails globalError)
					{
						diagnosticContext.Set("ExceptionType", globalError.ExceptionType ?? string.Empty);
						diagnosticContext.Set("ExceptionMessage", globalError.Message);
						diagnosticContext.Set("ExceptionStackTrace", globalError.StackTrace ?? string.Empty);
						diagnosticContext.Set("ExceptionStatus", globalError.Status);
					}
					diagnosticContext.Set("TraceId", httpContext.TraceIdentifier);
					diagnosticContext.Set("ConnectionId", httpContext.Connection.Id);
					diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
					diagnosticContext.Set("ResponseStatusCode", httpContext.Response.StatusCode);
					diagnosticContext.Set("RequestPath", httpContext.Request.Path);
					diagnosticContext.Set("RequestQueryString", httpContext.Request.QueryString);
					diagnosticContext.Set("RequestContentType", httpContext.Request.ContentType ?? string.Empty);
					diagnosticContext.Set("RequestContentLength", httpContext.Request.ContentLength ?? 0);
					diagnosticContext.Set("RequestHeaders", httpContext.Request.Headers.ToString() ?? string.Empty);
					diagnosticContext.Set("ResponseHeaders", httpContext.Response.Headers.ToString() ?? string.Empty);
					diagnosticContext.Set("RequestHost", httpContext.Request.Host);
					diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
					diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);
				};
			});
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

		/// <summary>
		/// Добавление EntityFrameworkDb для работы с БД.
		/// </summary>
		public static IServiceCollection AddMerchandiseServicesEntityFrameworkDb(this IServiceCollection services, IConfiguration configuration)
		{
			var dbConfigSection = configuration.GetSection("DatabaseConnectionOptions");
			var dbConfig = dbConfigSection.Get<DbConfiguration>();

			services.Configure<DbConfiguration>(configuration);
			services.AddDbContext<MerchandiseDbContext>(opt => { opt.UseNpgsql(dbConfig.ConnectionString); });
			return services;
		}

		/// <summary>
		/// Настраиваем порты для приложения.
		/// </summary>
		/// <param name="builder"></param>
		/// <returns></returns>
		public static WebApplicationBuilder ConfigurePorts(this WebApplicationBuilder builder)
		{
			var httpPortEnv = Environment.GetEnvironmentVariable("HTTP_PORT");
			if (!int.TryParse(httpPortEnv, out var httpPort))
				httpPort = 5006;

			var grpcPortEnv = Environment.GetEnvironmentVariable("GRPC_PORT");
			if (!int.TryParse(grpcPortEnv, out var grpcPort))
				grpcPort = 5008;

			if (Environment.GetEnvironmentVariable("DOTNET_RUNNING_IN_CONTAINER") == "true")
			{
				httpPort = 80;
				grpcPort = 82;
			}

			builder.WebHost.ConfigureKestrel(options =>
			{
				Listen(options, httpPort, HttpProtocols.Http1);
				Listen(options, grpcPort, HttpProtocols.Http2);
			});

			return builder;
		}

		/// <summary>
		/// Настраивает сервер Kestrel на прослушивание на указанном порту с использованием заданных протоколов.
		/// </summary>
		/// <param name="kestrelServerOptions">Параметры конфигурации сервера Kestrel.</param>
		/// <param name="port">Порт, на котором будет прослушиваться сервер.</param>
		/// <param name="protocols">Протоколы, которые будут использоваться для соединения.</param>
		static void Listen(KestrelServerOptions kestrelServerOptions, int? port, HttpProtocols protocols)
		{
			if (port == null)
				return;

			var address = IPAddress.Any;

			kestrelServerOptions.Listen(address, port.Value, listenOptions => { listenOptions.Protocols = protocols; });
		}
	}
}
