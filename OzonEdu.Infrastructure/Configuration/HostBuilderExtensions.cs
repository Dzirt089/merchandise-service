using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters;
using System.Runtime.CompilerServices;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration
{
    public static class HostBuilderExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
        {
            //Добавляю свои настройки для логов 
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConfiguration(configuration.GetSection("Logging"));
                builder.AddConsole();
                builder.AddFile(@"Logs/app-{Date}.txt");
            });

            services.AddSwaggerGen();
            services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
            services.AddSingleton<IStartupFilter, TerminalStartupFilter>();
            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

            return services;
        }
    }
}
