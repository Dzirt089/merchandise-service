﻿using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Filters;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Interceptors;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.Middlewares;
using OzonEdu.MerchandiseService.Infrastructure.Configuration.StartapFilters;

namespace OzonEdu.MerchandiseService.Infrastructure.Configuration
{
	public static class HostBuilderExtensions
	{
		//Создаю кофигурацию, чтобы библиотечке считать данные с неё
		static IConfigurationRoot? _configuration = new ConfigurationBuilder()
			.SetBasePath(Directory.GetCurrentDirectory())
			.AddJsonFile("appsettings.json")
			.Build();

		/// <summary>
		/// Подключаем вспомогательные сервисы (Swagger, Logging, <see cref="GlobalExceptionFilter"/> и т.д.
		/// </summary>
		/// <param name="services"></param>
		/// <returns></returns>
		public static IServiceCollection AddInfrastructure(this IServiceCollection services)
		{
			//Добавляю свои настройки для логов 
			if (_configuration != null)
			{
				services.AddLogging(builder =>
				{
					builder.ClearProviders();
					builder.AddConfiguration(_configuration.GetSection("Logging"));
					builder.AddConsole();
					builder.AddFile(@"Logs/app-{Date}.txt");
				});
			}


			services.AddSwaggerGen();
			services.AddSingleton<IStartupFilter, SwaggerStartupFilter>();
			services.AddSingleton<IStartupFilter, TerminalStartupFilter>();
			services.AddControllers(options => options.Filters.Add<GlobalExceptionFilter>());
			services.AddGrpc(services => services.Interceptors.Add<LoggingInterceptor>());

			return services;
		}

		/// <summary>
		/// Подключаем для логгирования Миддлеваре "RequestResponseLoggingMiddleware".   
		/// </summary>
		/// <param name="app"></param>
		/// <returns></returns>
		public static IApplicationBuilder AddInfrastructureMiddleware(this IApplicationBuilder app)
		{
			app.UseMiddleware<RequestResponseLoggingMiddleware>();
			return app;
		}
	}
}
