using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(7)]
	public class SkuPresetTypeIdIdx : ForwardOnlyMigration
	{
		public override void Up()
		{
			Create
				.Index("sku_preset_type_id_idx")
				.OnTable("skus")
				.InSchema("public")
				.OnColumn("preset_type_id");
		}
	}
}
