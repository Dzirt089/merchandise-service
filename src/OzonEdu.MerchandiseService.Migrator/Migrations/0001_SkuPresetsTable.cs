using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	[Migration(1)]
	public class SkuPresetsTable : Migration
	{
		public override void Up()
		{
			if (!TableExists(CommonConstants.SkuPresetTable))
			{
				Create.Table(CommonConstants.SkuPresetTable)
					.WithColumn("Id").AsInt64().PrimaryKey().Identity()
					.WithColumn("preset_type_id").AsInt32().NotNullable();
			}
		}

		public override void Down()
		{
			if (TableExists(CommonConstants.SkuPresetTable))
			{
				Delete.Table(CommonConstants.SkuPresetTable);
			}
		}

		private bool TableExists(string tableName, string tdmSchema = "public") =>
		 Schema.Schema(tdmSchema).Table(tableName).Exists();
	}
}
