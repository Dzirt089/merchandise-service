using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares
{
    public class RequestResponseLoggingMiddleware(RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;
        private readonly RecyclableMemoryStreamManager _recyclable = new();

        public async Task InvokeAsync(HttpContext context)
        {
            await LogRequest(context);            
            await LogResponse(context);
        }

        private async Task LogRequest(HttpContext context)
        {
            try
            {
                if (context.Request.ContentType == "application/grpc" || context.Request.ContentLength > 0) return;

                context.Request.EnableBuffering();
                using var streamRequest = _recyclable.GetStream();
                await context.Request.Body.CopyToAsync(streamRequest);
                var bodyAsText = Encoding.UTF8.GetString(streamRequest.ToArray());

                _logger.LogInformation
                    (
                        @$"Http Request Information:{Environment.NewLine},
                        Body : {bodyAsText},
                        Headers : {context.Request.Headers},
                        Route : {context.Request.Host}"
                    );

                context.Request.Body.Position = 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log request body");
            }
        }

        private async Task LogResponse(HttpContext context)
        {
            try
            {
                if (context.Response.ContentType == "application/grpc" || context.Response.ContentLength > 0) return;
                var originalResponseBody = context.Response.Body;

                using var streamResponse = _recyclable.GetStream();
                context.Response.Body = streamResponse;
                await _next(context);
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var text = await new StreamReader(context.Response.Body).ReadToEndAsync();

                context.Response.Body.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation
                    (
                        @$"Http Response Information : {Environment.NewLine},
                        Body : {text},
                        Headers : {context.Request.Headers},
                        Route : {context.Request.Host}"
                    );

                await streamResponse.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log Response body");
            }
        }
    }
}
