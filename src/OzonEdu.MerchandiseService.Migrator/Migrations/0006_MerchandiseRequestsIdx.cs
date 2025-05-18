using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(6, TransactionBehavior.None)]
	public class MerchandiseRequestsIdx : ForwardOnlyMigration
	{
		public override void Up()
		{
			Create.Index("IX_merchandise_requests_sku_preset_id")
			   .OnTable("merchandise_requests")
			   .OnColumn("sku_preset_id");
		}
	}
}
