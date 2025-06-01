using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	/// <summary>
	/// Таблица Пересетов (WelcomePack и т.д.)
	/// </summary>
	[Migration(4)]
	public class PresetTypeTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("preset_types")
				.WithColumn("id").AsInt32().PrimaryKey()
				.WithColumn("name").AsString().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("preset_types");
		}
	}
}
