using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace OzonEdu.Infrastructure.Configuration.Middlewares
{
    public class VersionMiddleware
    {
        private readonly RequestDelegate _next;
        public VersionMiddleware(RequestDelegate next) 
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            await GetVersionAndName(context);
            //TODO: Поэксперементировать с некстом и без
            //await _next(context);
        }

        private async Task GetVersionAndName(HttpContext context)
        {
            var version = Assembly.GetExecutingAssembly().GetName().Version?.ToString() ?? "no version";
            var name = Assembly.GetExecutingAssembly().GetName().Name?.ToString() ?? "no name";
            var versionRespons = new
            {
                Version = version,
                ServiceName = name,
            };
            var resultJson = new JsonResult(versionRespons);
            await context.Response.WriteAsync(JsonSerializer.Serialize(resultJson));
        }
    }
}
