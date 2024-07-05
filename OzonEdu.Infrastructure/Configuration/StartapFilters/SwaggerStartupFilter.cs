using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters
{
	public class SwaggerStartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return app =>
			{
				app.UseSwagger();
				app.UseSwaggerUI();
				next(app);
			};
		}
	}
}
