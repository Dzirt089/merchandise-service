﻿using FluentMigrator;

namespace OzonEdu.MerchandiseService.Migrator.Migrations
{
	/// <summary>
	/// Таблица размеров
	/// </summary>
	[Migration(2)]
	public class ClothingSizesTable : Migration
	{
		public override void Up()
		{
			Create
				.Table("clothing_sizes")
				.WithColumn("id").AsInt32().PrimaryKey()
				.WithColumn("name").AsString().NotNullable();
		}

		public override void Down()
		{
			Delete.Table("clothing_sizes");
		}

	}
}
