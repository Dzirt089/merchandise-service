using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OzonEdu.MerchandiseService.Infrastructure.Filters
{
	public class GlobalExceptionFilter(ILogger<GlobalExceptionFilter> logger) : ExceptionFilterAttribute
	{
		private readonly ILogger<GlobalExceptionFilter> _logger = logger;

		/// <summary>
		/// Обработчик исключений, который возвращает JSON-ответ с информацией об ошибке.
		/// </summary>
		/// <param name="context"></param>
		/// <returns></returns>
		public override void OnException(ExceptionContext context)
		{

			var isDevelopment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";

			var statusCode = context.Exception switch
			{
				ArgumentNullException => 400,
				_ => 500
			};

			//Создание объекта с подробностями о проблеме, чтобы вернуть его в ответе.
			var error = new GlobalErrorDetails
				(
					exceptionType: context.Exception.GetType().FullName,
					message: isDevelopment ? context.Exception.Message : "Произошла ошибка. Обратитесь в отдел IT",
					stackTrace: isDevelopment ? context.Exception.StackTrace : null,
					status: statusCode
				);

			var jsonResult = new JsonResult(error)
			{
				StatusCode = statusCode
			};

			// “прикарманиваем” объект с деталями ошибки в текущем HttpContext так, чтобы любой другой код (например, ваш EnrichDiagnosticContext в UseSerilogRequestLogging) мог эти детали прочитать и добавить в лог.
			context.HttpContext.Items["ErrorDetails"] = error;

			// Это то, что реально заставляет MVC-движок не бросать дальше исключение, а вернуть клиенту именно тот JSON-ответ, который выше сконструировали.
			context.Result = jsonResult;
		}
	}
}
