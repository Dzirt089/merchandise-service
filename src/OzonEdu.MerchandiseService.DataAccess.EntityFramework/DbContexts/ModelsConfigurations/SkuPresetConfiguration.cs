using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OzonEdu.MerchandiseService.Domain.AggregationModels.SkuPresets;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts.ModelsConfigurations
{
	internal class SkuPresetConfiguration : IEntityTypeConfiguration<SkuPreset>
	{
		public void Configure(EntityTypeBuilder<SkuPreset> builder)
		{
			builder.ToTable("sku_presets");
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

			// Коллекция SKU (Value Objects) в отдельной таблице
			builder.OwnsMany(x => x.SkuCollection, sku =>
			{
				sku.ToTable("sku_preset_skus");
				sku.WithOwner().HasForeignKey("sku_preset_id");

				sku.Property(x => x.Value)
					.HasColumnName("sku_value")
					.HasColumnType("bigint");

				// Составной ключ: sku_preset_id + sku_value
				sku.HasKey("sku_preset_id", nameof(Sku.Value));
			});

			//Методы изменяют коллекцию _skus, но EF Core не узнает об этом, если не настроено отслеживание изменений через приватное поле.
			//Это заставит EF Core отслеживать изменения в приватном поле _skus.
			builder.Metadata
				.FindNavigation(nameof(SkuPreset.SkuCollection))
				.SetPropertyAccessMode(PropertyAccessMode.Field);

			// PresetType
			builder.Property(x => x.Type)
				.HasConversion(
					x => x.Id, // Сохраняем Id
					x => PresetType.GetAll<PresetType>().First(p => p.Id == x)) // Восстанавливаем по Id
				.HasColumnName("preset_type_id"); // Колонка должна быть int
		}
	}
}
