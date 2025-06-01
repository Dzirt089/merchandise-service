using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(12)]
	public class SkuPresetSkusTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("sku_preset_skus")
				.WithColumn("sku_preset_id").AsInt32().NotNullable().PrimaryKey()
				.WithColumn("sku_id").AsInt64().NotNullable().PrimaryKey();
		}

		public override void Down()
		{
			Delete.Table("sku_presets");
		}
	}
}
