using Microsoft.Extensions.Diagnostics.HealthChecks;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts;

namespace OzonEdu.MerchandiseService.Infrastructure.HealthChecks
{
    //TODO: Посмотреть внимательнее DatabaseHealthCheck в OzonEdu.MerchandiseService.Infrastructure.HealthChecks
    public sealed class DatabaseHealthCheck : IHealthCheck
    {
        private readonly IServiceScopeFactory _serviceScopeFactory;

        public DatabaseHealthCheck(IServiceScopeFactory serviceScopeFactory)
        {
            _serviceScopeFactory = serviceScopeFactory;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(
            HealthCheckContext context,
            CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _serviceScopeFactory.CreateScope();
                var dbContext = scope.ServiceProvider.GetRequiredService<MerchandiseDbContext>();
                var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

                return canConnect
                    ? HealthCheckResult.Healthy("PostgreSQL is reachable.")
                    : HealthCheckResult.Unhealthy("PostgreSQL is not reachable.");
            }
            catch (Exception exception)
            {
                return HealthCheckResult.Unhealthy("PostgreSQL health check failed.", exception);
            }
        }
    }
}
