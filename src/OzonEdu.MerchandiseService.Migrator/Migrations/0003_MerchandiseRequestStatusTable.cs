using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	/// <summary>
	/// Таблица статусов
	/// </summary>
	[Migration(3)]
	public class MerchandiseRequestStatusTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("merchandise_request_status")
				.WithColumn("id").AsInt32().PrimaryKey()
				.WithColumn("name").AsString().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("merchandise_request_status");
		}
	}
}
