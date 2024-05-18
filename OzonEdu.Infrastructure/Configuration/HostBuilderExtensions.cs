using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration
{
    public static class HostBuilderExtensions
    {
        public static IServiceCollection AddInfrastructure(this IServiceCollection services)
        {
            services.AddSwaggerGen();
            services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
            services.AddSingleton<IStartupFilter, TerminalStartupFilter>();

            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

            return services;
        }
    }
}
