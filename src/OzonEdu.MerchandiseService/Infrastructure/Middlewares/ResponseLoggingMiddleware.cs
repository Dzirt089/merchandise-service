using OzonEdu.MerchandiseService.Infrastructure.Interceptors;

using System.Text.Json;

namespace OzonEdu.MerchandiseService.Infrastructure.Middlewares
{
	internal class ResponseLoggingMiddleware
	{
		private readonly RequestDelegate _next;
		private readonly ILogger<ResponseLoggingMiddleware> _logger;

		public ResponseLoggingMiddleware(RequestDelegate next, ILogger<ResponseLoggingMiddleware> logger)
		{
			_next = next;
			_logger = logger;
		}

		public async Task InvokeAsync(HttpContext context)
		{
			// Если пришёл gRPC-запрос, просто пропускаем его дальше без логирования ответа
			if (string.Equals(context.Request.ContentType, "application/grpc", StringComparison.OrdinalIgnoreCase))
			{
				await _next(context);
				return;
			}
			//Сохраняем ссылку на исходный поток Response.Body, который по умолчанию пишет сразу в выходной HTTP-поток (на клиент).
			var originalBodyStream = context.Response.Body;

			//Создаём новый MemoryStream — временный буфер в памяти.
			await using var responseBody = new MemoryStream();

			//Перенаправляем context.Response.Body на наш MemoryStream. Теперь всё, что приложение будет «писать» в ответ, в действительности попадёт в эту память, а не уйдёт сразу клиенту.
			context.Response.Body = responseBody;

			// Передаём управление следующему middleware в конвейере. В данном случае это контроллер, который сгенерирует ответ.
			await _next(context);

			try
			{
				// Ставим курсор на начало буфера
				context.Response.Body.Seek(0, SeekOrigin.Begin);

				// Считываем весь текст ответа
				var text = await new StreamReader(context.Response.Body).ReadToEndAsync();

				// Снова сбрасываем курсор, чтобы копировать весь контент
				context.Response.Body.Seek(0, SeekOrigin.Begin);

				// Экранируем спецсимволы через JSON-сериализацию
				var serializedJsonOutput =
					JsonSerializer.Serialize(text, JsonSerializerOptionsFactory.Default);

				_logger.LogInformation("Request logged");
				_logger.LogInformation(serializedJsonOutput);

				// Копируем из нашего буфера в исходный поток,
				// чтобы клиент получил ответ
				await responseBody.CopyToAsync(originalBodyStream);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Could not log response");
			}
		}
	}
}
