using FluentMigrator;

using System.Data;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(5)]
	public class ForeignKey_SkuPresetSkusTable : Migration
	{

		public override void Up()
		{
			Create.ForeignKey("FK_sku_preset_skus_sku_presets")
				.FromTable("sku_preset_skus")
				.ForeignColumn("sku_preset_id")
				.ToTable("sku_presets")
				.PrimaryColumn("Id")
				.OnDelete(Rule.Cascade);
		}

		public override void Down()
		{
			Delete.ForeignKey("FK_sku_preset_skus_sku_presets").OnTable("sku_preset_skus");
		}

	}
}
