using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(3)]
	public class SkuPresetSkusTable : Migration
	{
		public override void Up()
		{
			if (!TableExists(CommonConstants.SkuPresetSkusTable))
			{
				Create.Table(CommonConstants.SkuPresetSkusTable)
					.WithColumn("sku_preset_id").AsInt64().NotNullable()
					.WithColumn("sku_value").AsInt64().NotNullable();

				// Primary Key для составного ключа
				Create.PrimaryKey("PK_sku_preset_skus")
					.OnTable(CommonConstants.SkuPresetSkusTable)
					.Columns("sku_preset_id", "sku_value");
			}
		}

		public override void Down()
		{
			if (TableExists(CommonConstants.SkuPresetSkusTable))
			{
				Delete.Table(CommonConstants.SkuPresetSkusTable);
			}
		}

		private bool TableExists(string tableName, string tdmSchema = "public") =>
		 Schema.Schema(tdmSchema).Table(tableName).Exists();
	}
}
