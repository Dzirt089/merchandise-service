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

            services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
            services.AddSingleton<IStartupFilter, TerminalStartupFilter>();
            services.AddSwaggerGen(options =>
            {
                options.SwaggerDoc("v1", new Microsoft.OpenApi.Models.OpenApiInfo { Title = "OzonEdu.MerchandiseService", Version = "v1" });
                options.CustomSchemaIds(x => x.FullName);
            });
            services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());

            return services;
        }
    }
}
