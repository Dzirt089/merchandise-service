using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(6)]
	public class MerchandiseRequestsTable : Migration
	{
		public override void Up()
		{
			if (!TableExists(CommonConstants.MerchandiseRequestsTable))
			{
				Create.Table("merchandise_requests")
					.WithColumn("id").AsInt64().PrimaryKey().Identity()
					.WithColumn("sku_preset_id").AsInt64().NotNullable()
					.WithColumn("merchandise_request_status").AsString().NotNullable()
					.WithColumn("created_at").AsDateTimeOffset().NotNullable()
					.WithDefaultValue(SystemMethods.CurrentUTCDateTime)
					.WithColumn("give_out_at").AsDateTimeOffset().Nullable()
					.WithColumn("clothing_size").AsString().Nullable()
					.WithColumn("employee_email").AsString().NotNullable();
			}
		}

		public override void Down()
		{
			if (TableExists(CommonConstants.MerchandiseRequestsTable))
			{
				Delete.Table(CommonConstants.MerchandiseRequestsTable);
			}
		}

		private bool TableExists(string tableName, string tdmSchema = "public") =>
		 Schema.Schema(tdmSchema).Table(tableName).Exists();
	}
}
