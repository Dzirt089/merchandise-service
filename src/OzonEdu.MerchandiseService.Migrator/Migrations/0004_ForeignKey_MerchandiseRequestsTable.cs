using FluentMigrator;

using System.Data;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(4)]
	public class ForeignKey_MerchandiseRequestsTable : Migration
	{
		public override void Up()
		{
			Create.ForeignKey("FK_merchandise_requests_sku_presets")
			   .FromTable("merchandise_requests").ForeignColumn("sku_preset_id")
			   .ToTable("sku_presets").PrimaryColumn("Id")
			   .OnDelete(Rule.Cascade);
		}
		public override void Down()
		{
			Delete.ForeignKey("FK_merchandise_requests_sku_presets")
				.OnTable("merchandise_requests");
		}

	}
}
