using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(1)]
	public class SkuTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("skus")
				.WithColumn("id").AsInt64().PrimaryKey()
				.WithColumn("name").AsString().NotNullable()
				.WithColumn("item_type_id").AsInt32().NotNullable()
				.WithColumn("clothing_size").AsInt32().Nullable()
				.WithColumn("preset_type_id").AsInt32().NotNullable();

		}

		public override void Down()
		{
			Execute.Sql("DROP TABLE if exists skus;");
		}
	}
}
