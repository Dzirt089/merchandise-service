﻿using Microsoft.EntityFrameworkCore;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts.ModelsConfigurations;
using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;
using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;
using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts
{
	public class MerchandiseDbContext : DbContext
	{
		public DbSet<MerchandiseRequest> MerchandiseRequests => Set<MerchandiseRequest>();
		public DbSet<SkuPreset> SkuPresets => Set<SkuPreset>();

		public DbSet<SkuDbModel> Skus => Set<SkuDbModel>();

		public MerchandiseDbContext(DbContextOptions<MerchandiseDbContext> options) : base(options) { }

		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			modelBuilder.ApplyConfiguration(new MerchandiseRequestConfiguration());
			modelBuilder.ApplyConfiguration(new SkuPresetConfiguration());

			modelBuilder.Entity<SkuDbModel>(b =>
			{
				b.ToTable("skus");
				b.HasKey(x => x.Id);
				b.Property(x => x.Id).HasColumnName("id");

				b.Property(x => x.ItemTypeId)
					.HasColumnName("item_type_id")
					.IsRequired();

				b.Property(x => x.ClothingSize)
					.HasColumnName("clothing_size");

				b.Property(x => x.PresetTypeId)
					.HasColumnName("preset_type_id")
					.IsRequired();

				b.Property(x => x.Name)
					.HasColumnName("name")
					.IsRequired();
			});
		}
	}
}

//// Переводчик для обычных дат (не null)
//var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
//	v => v.ToUniversalTime(), // При сохранении в БД: переводим в UTC (всемирное время)
//	v => DateTime.SpecifyKind(v.ToUniversalTime(), DateTimeKind.Utc)); // При чтении из БД: помечаем как UTC

//// Переводчик для дат, которые могут быть null (например, дата увольнения)
//var nullableDateTimeConverter = new ValueConverter<DateTime?, DateTime?>(
//	v => v.HasValue ? v.Value.ToUniversalTime() : v, // Проверяем, не null ли, прежде чем переводить
//	v => v.HasValue ? DateTime.SpecifyKind(v.Value.ToUniversalTime(), DateTimeKind.Utc) : v);

//foreach (var entityType in modelBuilder.Model.GetEntityTypes())
//{
//	// Пропускаем "бесполые" сущности (без первичного ключа)
//	if (entityType.IsKeyless) continue;

//	// Проверяем каждое свойство (столбец) в таблице
//	foreach (var property in entityType.GetProperties())
//	{
//		if (property.ClrType == typeof(DateTime)) // Если это обычная дата
//		{
//			property.SetValueConverter(dateTimeConverter); // Применяем переводчик
//		}
//		else if (property.ClrType == typeof(DateTime?)) // Если дата может быть null
//		{
//			property.SetValueConverter(nullableDateTimeConverter); // Переводчик для nullable
//		}
//	}
//}