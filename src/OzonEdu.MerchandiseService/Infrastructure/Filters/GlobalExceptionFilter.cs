using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace OzonEdu.MerchandiseService.Infrastructure.Filters
{
	public class GlobalExceptionFilter : ExceptionFilterAttribute
	{
		/// <summary>
		/// Для IExceptionFilter можно переопределить только один метод.
		/// </summary>
		/// <param name="context"></param>
		public override void OnException(ExceptionContext context)
		{
			//Создание объекта с подробностями о проблеме, чтобы вернуть его в ответе.
			var error = new ProblemDetails
			{
				Title = "An error occurred",
				Detail = context.Exception.Message,
				Status = 500,
				Type = "https://httpstatuses.com/500"
			};
			//Создает ObjectResult для сериализации ProblemDetails и установки кода состояния ответа
			context.Result = new ObjectResult(error) { StatusCode = 500 };

			//Помечает исключение как обработанное, чтобы предотвратить его распространение в конвейер промежуточного ПО.
			context.ExceptionHandled = true;
		}
	}
}
