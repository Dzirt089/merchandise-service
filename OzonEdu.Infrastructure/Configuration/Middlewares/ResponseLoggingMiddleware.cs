using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares
{
    public class ResponseLoggingMiddleware(RequestDelegate next,
        ILogger<ResponseLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            await LogResponse(context);
            await _next(context);
        }

        private async Task LogResponse(HttpContext context)
        {
            try
            {
                if (context.Response.ContentType == "application/grpc") return;
                if (context.Response.ContentLength > 0)
                {
                    var buffer = new byte[context.Response.ContentLength.Value];
                    await context.Response.Body.ReadAsync(buffer, 0, buffer.Length);
                    var bodyAsText = Encoding.UTF8.GetString(buffer);

                    _logger.LogInformation(
                    @$"Http Response Information : {Environment.NewLine}
                    Body : {bodyAsText}
                    Headers : {context.Request.Headers}
                    Route : {context.Request.Host}");

                    context.Response.Body.Position = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log Response body");
            }


        }
    }
}
