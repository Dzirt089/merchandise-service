using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.OpenApi.Models;

using OzonEdu.MerchandiseService.Infrastructure.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Interceptors;
using OzonEdu.MerchandiseService.Infrastructure.StartapFilters;

using Serilog;
using Serilog.Events;

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
