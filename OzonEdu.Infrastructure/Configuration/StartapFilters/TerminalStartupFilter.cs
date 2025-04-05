using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;

using System.Reflection;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters
{
	public class TerminalStartupFilter : IStartupFilter
	{
		public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
		{
			return app =>
			{
				app.Map("/live", b => b.Run(async liveOk => await liveOk.Response.CompleteAsync()));
				app.Map("/ready", b => b.Run(async readyOk => await readyOk.Response.CompleteAsync()));
				app.Map("/version", b => b.Run(async version =>
				{
					var versionRespons = new
					{
						Version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version",
						ServiceName = Assembly.GetExecutingAssembly().GetName().Name?.ToString() ?? "no name",
					};
					await version.Response.WriteAsJsonAsync(versionRespons);
				}));

				var apps = app.Build();

				next(app);
			};
		}
	}
}

