using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(11)]
	public class SkuPresetsTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("sku_presets")
				.WithColumn("id").AsInt32().PrimaryKey().Identity()
				.WithColumn("preset_type_id").AsInt32().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("sku_presets");
		}
	}
}
