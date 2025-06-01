using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	/// <summary>
	/// Тип мерча (блокнот, ручка, кофта и т.д.)
	/// </summary>
	[Migration(5)]
	public class ItemTypes : Migration
	{
		public override void Up()
		{
			Create
				.Table("item_types")
				.WithColumn("id").AsInt32().PrimaryKey()
				.WithColumn("name").AsString().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("item_types");
		}
	}
}
