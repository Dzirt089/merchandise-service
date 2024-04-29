using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.Infrastructure.Configuration.Middlewares
{
    public class ResponseLoggingMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ResponseLoggingMiddleware> _logger;
        public ResponseLoggingMiddleware(RequestDelegate next,
            ILogger<ResponseLoggingMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await LogResponse(context);
            await _next(context);
        }

        private async Task LogResponse(HttpContext context)
        {
            try
            {
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
