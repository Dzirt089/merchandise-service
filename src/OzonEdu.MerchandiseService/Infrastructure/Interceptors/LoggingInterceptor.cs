using Grpc.Core;
using Grpc.Core.Interceptors;

using System.Text.Json;
namespace OzonEdu.MerchandiseService.Infrastructure.Interceptors
{
	/// <summary>
	/// Interceptor - компонент в логгировании grpc, который получает доступ к уже десереализованному сообщению.
	/// </summary>
	/// <param name="logger"></param>
	public class LoggingInterceptor(ILogger<LoggingInterceptor> logger) : Interceptor
	{
		private readonly ILogger<LoggingInterceptor> _logger = logger;

		/// <summary>
		/// Логируем обычные gRPC запросы
		/// </summary>
		public override async Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
			ServerCallContext context,
			UnaryServerMethod<TRequest, TResponse> continuation)
		{
			// Логирование запроса
			try
			{
				// название метода, который вызывают
				_logger.LogInformation($"Grpc request {context.Method}");

				// Сериализуем request (Protobuf-сообщение) в JSON.
				var requestJson = JsonSerializer.Serialize(request, JsonSerializerOptionsFactory.Default);

				//Записываем в лог.
				_logger.LogInformation(requestJson);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Could not log grpc request");
			}

			// Вызов реального метода в сервисе, то есть метод сервиса, например "/ozon.stockapi.Stock/GetStockById"
			var response = await base.UnaryServerHandler(request, context, continuation);

			// Логирование ответа
			try
			{
				// Сериализуем response (protobuf-объект) в JSON. Затем логируем его.
				var responseJson = JsonSerializer.Serialize(response, JsonSerializerOptionsFactory.Default);
				_logger.LogInformation(responseJson);
			}
			catch (Exception exception)
			{
				_logger.LogError(exception, "Could not log grpc response");
			}

			// Возвращаем ответ клиенту
			return response;
		}

		/// <summary>
		/// Логгируем стримы. Для сбора статистики, как часто стримы открываются, например.
		/// </summary>
		public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
			ClientInterceptorContext<TRequest, TResponse> context,
			AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
		{
			_logger.LogInformation("Streaming has been called");

			return base.AsyncServerStreamingCall(request, context, continuation);
		}
	}
}
