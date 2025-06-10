using FluentMigrator.Runner;

using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var configuration = new ConfigurationBuilder()
	.SetBasePath(Directory.GetCurrentDirectory())
	.AddJsonFile("appsettings.json")
	.AddEnvironmentVariables()
	.Build();

var connectionString = configuration.GetSection("DatabaseConnectionOptions:ConnectionString").Get<string>();

//Контейнер с необходимым сервисом для запуска миграций
var services = new ServiceCollection()
	.AddFluentMigratorCore() //регистрирует Fluent Migrator и все сервисы для него
	.ConfigureRunner(
		r =>
					r.AddPostgres() //Конфигурируя, указываем что работаем с Postgres
						.WithGlobalConnectionString(connectionString) //Указываем глобально строку подключения
						.ScanIn(typeof(Program).Assembly) //Указываем где необходимо искать миграцию, аргумент - сборка
						.For.Migrations());

//Штука, которая рулит всем этим делом
var serviceProvider = services.BuildServiceProvider();

using (serviceProvider.CreateScope())
{
	var runner = serviceProvider.GetRequiredService<IMigrationRunner>();

	if (args.Contains("--dryrun")) //строка "--dryrun", значит пользователь явно попросил не выполнять миграции, а просто посмотреть, что есть.
		runner.ListMigrations(); //ListMigrations() выводит в консоль список всех миграций, которые были найдены при сканировании сборки (делали .ScanIn(typeof(Program).Assembly).For.Migrations()), вместе с их номерами и статусом (применена/не применена). Это очень удобно, когда надо убедиться, какие конкретно миграции есть в проекте и в каком порядке они будут применяться, не рискуя изменить схему базы.
	else
		runner.MigrateUp(); //Это и есть «боевой» запуск: все миграции с номерами, больших чем текущая версия в таблице VersionInfo, будут применены одна за другой.
}