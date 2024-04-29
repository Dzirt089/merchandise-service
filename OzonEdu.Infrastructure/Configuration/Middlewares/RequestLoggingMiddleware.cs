using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System.Text;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares
{
    public class RequestLoggingMiddleware(RequestDelegate next,
        ILogger<RequestLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestLoggingMiddleware> _logger = logger;

        public async Task InvokeAsync(HttpContext context)
        {
            await LogRequest(context);
            await _next(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            try
            {
                if (context.Response.ContentType == "application/grpc") return;

                if (context.Request.ContentLength > 0)
                {
                    context.Request.EnableBuffering();

                    var bufferBody = new byte[context.Request.ContentLength.Value];
                    await context.Request.Body.ReadAsync(bufferBody, 0, bufferBody.Length);
                    var bodyAsText = Encoding.UTF8.GetString(bufferBody);

                    _logger.LogInformation(
                    @$"Http Request Information:{Environment.NewLine}
                    Body : {bodyAsText}
                    Headers : {context.Request.Headers}
                    Route : {context.Request.Host}");

                    context.Request.Body.Position = 0;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log request body");
            }


        }
    }
}
