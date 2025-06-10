using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(8)]
	public class MerchandiseRequestsEmployeeEmailIdx : ForwardOnlyMigration
	{
		public override void Up()
		{
			Create
				.Index("merchandise_requests_employee_email_idx")
				.OnTable("merchandise_requests")
				.InSchema("public")
				.OnColumn("employee_email");
		}
	}
}
