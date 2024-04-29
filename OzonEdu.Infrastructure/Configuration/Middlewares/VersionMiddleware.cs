using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Reflection;
using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares
{
    public class VersionMiddleware(RequestDelegate next, ILogger<VersionMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<VersionMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            await GetVersionAndName(context);
            //TODO: Поэксперементировать с некстом и без
            //await _next(context);
        }

        private async Task GetVersionAndName(HttpContext context)
        {
            try
            {
                if (context.Response.ContentType == "application/grpc") return;
                var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version";
                var name = Assembly.GetExecutingAssembly().GetName().Name?.ToString() ?? "no name";
                var versionRespons = new
                {
                    Version = version,
                    ServiceName = name,
                };
                var resultJson = new JsonResult(versionRespons);
                await context.Response.WriteAsync(JsonSerializer.Serialize(resultJson));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log Response body");
            }

        }
    }
}
