using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OzonEdu.MerchandiseService.Infrastructure.Filters
{
	public class GlobalExceptionFilter : ExceptionFilterAttribute
	{
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
			var error = new
			{
				ExceptionType = context.Exception.GetType().FullName,
				// Сообщение ошибки (безопасно для прода)
				Message = isDevelopment ? context.Exception.Message : "Произошла ошибка. Обратитесь в отдел IT",

				// Стектрейс ошибки (безопасно для прода)
				StackTrace = isDevelopment ? context.Exception.StackTrace : null,

				Status = statusCode
			};

			var jsonResult = new JsonResult(error)
			{
				StatusCode = statusCode
			};
			context.Result = jsonResult;
		}
	}
}
