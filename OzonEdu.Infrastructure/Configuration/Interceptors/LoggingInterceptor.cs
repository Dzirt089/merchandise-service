using Grpc.Core;
using Grpc.Core.Interceptors;
using Microsoft.Extensions.Logging;
using System.Text.Json;
namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.Interceptors
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
		/// <typeparam name="TRequest"></typeparam>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <param name="continuation"></param>
		/// <returns></returns>
		public override Task<TResponse> UnaryServerHandler<TRequest, TResponse>(TRequest request,
			ServerCallContext context,
			UnaryServerMethod<TRequest, TResponse> continuation)
		{
			try
			{
				var requestJson = JsonSerializer.Serialize(request);
				_logger.LogInformation(requestJson);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.StackTrace);
			}

			var response = base.UnaryServerHandler(request, context, continuation);
			try
			{
				var responseJson = JsonSerializer.Serialize(response);
				_logger.LogInformation(responseJson);
			}
			catch (Exception ex)
			{
				_logger.LogError(ex, ex.StackTrace);
			}

			return response;
		}

		/// <summary>
		/// Логгируем стримы. Для сбора статистики, как часто стримы открываются, например.
		/// </summary>
		/// <typeparam name="TRequest"></typeparam>
		/// <typeparam name="TResponse"></typeparam>
		/// <param name="request"></param>
		/// <param name="context"></param>
		/// <param name="continuation"></param>
		/// <returns></returns>
		public override AsyncServerStreamingCall<TResponse> AsyncServerStreamingCall<TRequest, TResponse>(TRequest request,
			ClientInterceptorContext<TRequest, TResponse> context,
			AsyncServerStreamingCallContinuation<TRequest, TResponse> continuation)
		{
			_logger.LogInformation("Streaming has been called");

			return base.AsyncServerStreamingCall(request, context, continuation);
		}
	}
}
