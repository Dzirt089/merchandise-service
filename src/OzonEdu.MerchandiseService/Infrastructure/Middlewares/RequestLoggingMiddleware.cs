using OzonEdu.MerchandiseService.Infrastructure.Interceptors;

using System.Text;
using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Middlewares
{
	internal class RequestLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<RequestLoggingMiddleware> _logger;

		public RequestLoggingMiddleware(RequestDelegate next, ILogger<RequestLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Пропускаем gRPC-запросы без логирования тела
			if (string.Equals(context.Request.ContentType, "application/grpc", StringComparison.OrdinalIgnoreCase))
			{
				await _next(context);
				return;
			}

			await LogRequest(context); // логируем запрос
			await _next(context); // передаём управление дальше
		}

		private async Task LogRequest(HttpContext context)
		{
			try
			{
				_logger.LogInformation($"Http request {context.Request.Path}");

				if (context.Request.ContentLength > 0)
				{
					// 1) Включаем буферизацию тела запроса
					context.Request.EnableBuffering();

					// 2) Читаем весь body в байтовый массив
					var buffer = new byte[context.Request.ContentLength.Value];
					await context.Request.Body.ReadAsync(buffer, 0, buffer.Length);

					// 3) Превращаем байты в строку
					var bodyAsText = Encoding.UTF8.GetString(buffer);

					// 4) Сериализуем её ещё раз в JSON (заключаем в кавычки, экранируем спецсимволы)
					var serializedJsonOutput =
						JsonSerializer.Serialize(bodyAsText, JsonSerializerOptionsFactory.Default);

					_logger.LogInformation("Request logged");
					_logger.LogInformation(serializedJsonOutput);

					// 5) Сбрасываем позицию потока, чтобы следующие middleware
					//    и конечная точка тоже смогли прочитать тело
					context.Request.Body.Position = 0;
				}
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Could not log request body");
			}
		}
	}
}
