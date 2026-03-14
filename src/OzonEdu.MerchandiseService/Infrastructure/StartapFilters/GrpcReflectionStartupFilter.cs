namespace OzonEdu.MerchandiseService.Infrastructure.StartapFilters
{
    public sealed class GrpcReflectionStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                next(app);

                var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
                if (!environment.IsDevelopment() && !environment.IsEnvironment("Test"))
                {
                    return;
                }

                app.UseEndpoints(endpoints => endpoints.MapGrpcReflectionService());
            };
        }
    }
}
