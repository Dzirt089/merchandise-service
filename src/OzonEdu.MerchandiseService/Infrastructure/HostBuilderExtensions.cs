using Grpc.Net.Client;
using Microsoft.AspNetCore.Server.Kestrel.Core;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;
using OpenTelemetry.Exporter;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;
using OzonEdu.MerchandiseService.Application.Abstractions.Integration;
using OzonEdu.MerchandiseService.Application.Contracts;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Configurations;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;
using OzonEdu.MerchandiseService.Domain.Root.Diagnostics;
using OzonEdu.MerchandiseService.Infrastructure.Filters;
using OzonEdu.MerchandiseService.Infrastructure.HealthChecks;
using OzonEdu.MerchandiseService.Infrastructure.Integration;
using OzonEdu.MerchandiseService.Infrastructure.Interceptors;
using OzonEdu.MerchandiseService.Infrastructure.Kafka;
using OzonEdu.MerchandiseService.Infrastructure.StartapFilters;
using OzonEdu.StockApi.Grpc;
using Prometheus;
using Serilog;
using Serilog.Events;
using System.Diagnostics;
using System.Net;
using System.Reflection;

namespace OzonEdu.MerchandiseService.Infrastructure
{
    //TODO: Посмотреть внимательнее HostBuilderExtensions в OzonEdu.MerchandiseService.Infrastructure
    public static class HostBuilderExtensions
    {
        public static IServiceCollection AddStockGrpcServiceClient(this IServiceCollection services, IConfiguration configuration)
        {
            var stockApiGrpcServiceConfiguration = configuration.GetSection(nameof(StockApiGrpcServiceConfiguration))
                .Get<StockApiGrpcServiceConfiguration>() ?? new StockApiGrpcServiceConfiguration();

            var connectionAddres = stockApiGrpcServiceConfiguration.ServerAddress;
            services.AddScoped<StockApiGrpc.StockApiGrpcClient>(_ =>
            {
                var channel = GrpcChannel.ForAddress(connectionAddres);
                return new StockApiGrpc.StockApiGrpcClient(channel);
            });

            return services;
        }

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
            services.AddSingleton<IStartupFilter, GrpcReflectionStartupFilter>();
            services.AddHealthChecks()
                .AddCheck<DatabaseHealthCheck>("postgres", failureStatus: HealthStatus.Unhealthy, tags: ["ready"])
                .AddCheck<KafkaHealthCheck>("kafka", failureStatus: HealthStatus.Unhealthy, tags: ["ready"]);
            services.AddGrpcReflection();

            return services;
        }

        public static WebApplicationBuilder AddInfrastructureLogger(this WebApplicationBuilder app)
        {
            var logPath = Path.Combine(Directory.GetCurrentDirectory(), "Logs");
            Directory.CreateDirectory(logPath);
            app.Services.AddInfrastructureKafka(app.Configuration);

            app.Host.UseSerilog((context, _, config) =>
            {
                config.ReadFrom.Configuration(context.Configuration)
                    .Enrich.FromLogContext()
                    .Enrich.WithProperty("Application", context.HostingEnvironment.ApplicationName)
                    .Enrich.WithProperty("Environment", context.HostingEnvironment.EnvironmentName);

                if (context.HostingEnvironment.IsDevelopment())
                {
                    config.WriteTo.Debug();
                }
            });

            return app;
        }

        public static WebApplicationBuilder AddInfrastructureOpenTelemetry(this WebApplicationBuilder app)
        {
            app.Services.AddOpenTelemetry()
                .ConfigureResource(resourceBuilder => resourceBuilder
                    .AddService(
                        serviceName: app.Environment.ApplicationName,
                        serviceVersion: Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version",
                        serviceInstanceId: Environment.MachineName))
                .WithTracing(tracingBuilder =>
                {
                    tracingBuilder
                        .AddSource(MerchandiseTelemetry.SourceName)
                        .AddAspNetCoreInstrumentation(options =>
                        {
                            options.Filter = context =>
                                !context.Request.Path.StartsWithSegments("/health")
                                && !context.Request.Path.StartsWithSegments("/ready")
                                && !context.Request.Path.StartsWithSegments("/live")
                                && !context.Request.Path.StartsWithSegments("/version");
                        })
                        .AddHttpClientInstrumentation()
                        .AddGrpcClientInstrumentation()
                        .AddEntityFrameworkCoreInstrumentation()
                        .AddSqlClientInstrumentation()
                        .AddConsoleExporter()
                        .AddOtlpExporter(options =>
                        {
                            var endpoint = Environment.GetEnvironmentVariable("OTEL_EXPORTER_OTLP_ENDPOINT")
                                ?? app.Configuration["OpenTelemetry:OtlpEndpoint"]
                                ?? "http://localhost:4317";

                            options.Endpoint = new Uri(endpoint);
                            options.Protocol = OtlpExportProtocol.Grpc;
                        });
                });

            return app;
        }

        public static IServiceCollection AddInfrastructureKafka(this IServiceCollection services, IConfiguration configuration)
        {
            services.Configure<KafkaOptions>(configuration.GetSection("Kafka"));
            services.AddScoped<IIntegrationOutboxWriter, EntityFrameworkIntegrationOutboxWriter>();
            services.AddHostedService<KafkaOutboxPublisherBackgroundService>();
            services.AddHostedService<StockReplenishedConsumerBackgroundService>();
            services.AddHostedService<EmployeeNotificationConsumerBackgroundService>();

            return services;
        }

        public static IApplicationBuilder AddInfrastructureMiddlewareHttp(this IApplicationBuilder app)
        {
            app.UseHttpMetrics();
            app.UseSerilogRequestLogging(options =>
            {
                options.MessageTemplate = "HTTP {RequestMethod} {RequestPath} responded {StatusCode} in {Elapsed:0.0000} ms";

                options.GetLevel = (httpContext, _, exception) =>
                {
                    if (exception != null || httpContext.Response.StatusCode >= 500)
                        return LogEventLevel.Error;
                    if (httpContext.Response.StatusCode >= 400)
                        return LogEventLevel.Warning;
                    return LogEventLevel.Information;
                };

                options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
                {
                    if (httpContext.Items.TryGetValue("ErrorDetails", out var errorDetails)
                        && errorDetails is GlobalErrorDetails globalError)
                    {
                        diagnosticContext.Set("ExceptionType", globalError.ExceptionType ?? string.Empty);
                        diagnosticContext.Set("ExceptionMessage", globalError.Message);
                        diagnosticContext.Set("ExceptionStackTrace", globalError.StackTrace ?? string.Empty);
                        diagnosticContext.Set("ExceptionStatus", globalError.Status);
                    }

                    diagnosticContext.Set("TraceId", Activity.Current?.TraceId.ToString() ?? httpContext.TraceIdentifier);
                    diagnosticContext.Set("SpanId", Activity.Current?.SpanId.ToString() ?? string.Empty);
                    diagnosticContext.Set("ConnectionId", httpContext.Connection.Id);
                    diagnosticContext.Set("RequestMethod", httpContext.Request.Method);
                    diagnosticContext.Set("ResponseStatusCode", httpContext.Response.StatusCode);
                    diagnosticContext.Set("RequestPath", httpContext.Request.Path);
                    diagnosticContext.Set("RequestQueryString", httpContext.Request.QueryString);
                    diagnosticContext.Set("RequestHost", httpContext.Request.Host);
                    diagnosticContext.Set("RequestScheme", httpContext.Request.Scheme);
                    diagnosticContext.Set("RequestProtocol", httpContext.Request.Protocol);
                };
            });

            app.UseRouting();
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
                endpoints.MapMetrics("/metrics");
            });
            return app;
        }

        public static IServiceCollection AddInfrastructureMiddlewareGrpc(this IServiceCollection services)
        {
            services.AddGrpc(options => options.Interceptors.Add<LoggingInterceptor>());
            return services;
        }

        public static IServiceCollection AddMerchandiseServicesEntityFrameworkDb(this IServiceCollection services, IConfiguration configuration)
        {
            var dbConfigSection = configuration.GetSection("DatabaseConnectionOptions");
            var dbConfig = dbConfigSection.Get<DbConfiguration>();

            services.Configure<DbConfiguration>(configuration);
            services.AddDbContext<MerchandiseDbContext>(options => options.UseNpgsql(dbConfig.ConnectionString));
            return services;
        }

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

        private static void Listen(KestrelServerOptions kestrelServerOptions, int? port, HttpProtocols protocols)
        {
            if (port == null)
                return;

            kestrelServerOptions.Listen(IPAddress.Any, port.Value, listenOptions => listenOptions.Protocols = protocols);
        }
    }
}
