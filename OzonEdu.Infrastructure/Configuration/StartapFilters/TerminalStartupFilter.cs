using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters
{
    public class TerminalStartupFilter : IStartupFilter
    {
        public Action<IApplicationBuilder> Configure(Action<IApplicationBuilder> next)
        {
            return app =>
            {
                app.Map("/live", b =>b.Run(async liveOk => await liveOk.Response.CompleteAsync()));
                app.Map("/ready", b => b.Run(async readyOk => await readyOk.Response.CompleteAsync()));
                app.Map("/version", b => app.UseMiddleware<VersionMiddleware>());

                app.UseMiddleware<RequestResponseLoggingMiddleware>();
                next(app);
            };
        }
    }
}
