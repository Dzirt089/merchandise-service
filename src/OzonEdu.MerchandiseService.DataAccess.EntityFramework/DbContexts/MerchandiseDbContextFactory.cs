using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts
{
	public class MerchandiseDbContextFactory : IDesignTimeDbContextFactory<MerchandiseDbContext>
	{
		public MerchandiseDbContext CreateDbContext(string[] args)
		{
			// Загружаем конфигурацию из appsettings.json
			var configuration = new ConfigurationBuilder()
				.SetBasePath(Directory.GetCurrentDirectory())
				.AddJsonFile("appsettings.json")
				.AddEnvironmentVariables()
				.Build();

			var connectionString = configuration.GetSection("DatabaseConnectionOptions:ConnectionString").Get<string>();

			var optionsBuilder = new DbContextOptionsBuilder<MerchandiseDbContext>();
			optionsBuilder.UseNpgsql(connectionString);

			return new MerchandiseDbContext(optionsBuilder.Options);
		}
	}
}
