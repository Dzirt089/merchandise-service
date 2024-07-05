using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Microsoft.IO;
using System.Text;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares
{
    /// <summary>
    /// Middleware, который логгирует входящие и исходящие HTTP-запросы и ответы
    /// </summary>
    /// <param name="next">Делегат, который указывает следующую функцию в цепочке middleware</param>
    /// <param name="logger">Интерфейс для логирования</param>
    public class RequestResponseLoggingMiddleware(RequestDelegate next,
        ILogger<RequestResponseLoggingMiddleware> logger)
    {
        private readonly RequestDelegate _next = next;
        private readonly ILogger<RequestResponseLoggingMiddleware> _logger = logger;

        /// <summary>
        /// Использование (Естественно с using) RecyclableMemoryStreamManager, позволяет избежать утечки памяти при использовании потоков. 
        /// </summary>
        private readonly RecyclableMemoryStreamManager _recyclable = new();

        /// <summary>
        /// Основной метод, который вызывается при обработке запроса
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public async Task InvokeAsync(HttpContext context)
        {
            await LogRequest(context);            
            await LogResponse(context);
			await _next(context);
		}

        /// <summary>
        /// Метод для логирования информации о входящем запросе
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task LogRequest(HttpContext context)
        {
            try
            {
                if (context.Request.ContentType == "application/grpc" || context.Request.ContentLength == 0 || context.Request.ContentLength is null) return;

                //Включаем буфер. Чтобы мы могли перематывать Body как касету после чтения, обратно на начало.
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

        /// <summary>
        /// Метод для логирования информации об исходящем ответе
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        private async Task LogResponse(HttpContext context)
        {
            try
            {
                if (context.Response.ContentType == "application/grpc" || context.Response.ContentLength == 0 || context.Request.ContentLength is null) return;
                
                //копируем перед работой, содержимое стрима ответа. Иначе после работы с ним, ответ "потеряется".
                var originalResponseBody = context.Response.Body;

                using var streamResponse = _recyclable.GetStream();
                context.Response.Body = streamResponse;
                await _next(context);

                //Перемещаем указатель потока к началу перед чтением потока.
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                var text = await new StreamReader(context.Response.Body).ReadToEndAsync();

                //Снова "перематываем" поток обратно на начало. Чтобы потом записать данные обратно в поток.
                context.Response.Body.Seek(0, SeekOrigin.Begin);

                _logger.LogInformation
                    (
                        @$"Http Response Information : {Environment.NewLine},
                        Body : {text},
                        Headers : {context.Request.Headers},
                        Route : {context.Request.Host}"
                    );
                //Копируем исходный стрим ответа обратно в поток
                await streamResponse.CopyToAsync(originalResponseBody);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Could not log Response body");
            }
        }
    }
}
