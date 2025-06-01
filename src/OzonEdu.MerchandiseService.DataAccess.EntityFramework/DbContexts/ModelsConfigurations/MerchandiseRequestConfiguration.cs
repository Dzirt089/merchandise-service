using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OzonEdu.MerchandiseService.Domain.AggregationModels.MerchandiseRequests;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts.ModelsConfigurations
{
	public class MerchandiseRequestConfiguration : IEntityTypeConfiguration<MerchandiseRequest>
	{
		public void Configure(EntityTypeBuilder<MerchandiseRequest> builder)
		{
			builder.ToTable("merchandise_requests");
			builder.HasKey(r => r.Id);

			// Используем поля для всех бэкинг-филдов
			builder.UsePropertyAccessMode(PropertyAccessMode.Field);

			//Важно: UsePropertyAccessMode(Field) заставит EF пропустить конструкторы и сразу наполнить приватные поля(_employeeEmail, _clothingSize, _skuPresetId) через reflection.

			// Бэкинг-филд для skuPresetId
			builder.Property<long>("_skuPresetId")
				   .HasColumnName("sku_preset_id")
				   .IsRequired();

			builder.Property<string>("_employeeEmail")
				   .HasColumnName("employee_email")
				   .IsRequired()
				   .HasMaxLength(255);

			builder.Property<string>("_clothingSize")
				   .HasColumnName("clothing_size")
				   .IsRequired(false);

			// Проперти для статуса через ValueConverter
			builder.Property(r => r.Status)
				   .HasColumnName("merchandise_request_status_id")
				   .HasConversion(
					   v => v.Id,                            // to db: int
					   v => MerchandiseRequestStatus.Parse(v.ToString()))
				   .IsRequired();

			builder.Property(r => r.CreatedAt)
				   .HasColumnName("created_at")
				   .IsRequired();

			builder.Property(r => r.GiveOutAt)
				   .HasColumnName("give_out_at")
				   .IsRequired(false);

			// Навигация к SkuPreset через внешний ключ
			builder.HasOne(r => r.SkuPreset)
				   .WithMany()
				   .HasForeignKey("_skuPresetId");

			builder.OwnsOne(r => r.Employee, eb =>
			{
				eb.WithOwner();  // важно для owned

				// Email
				eb.Property(e => e.Email)
				  .HasColumnName("employee_email")
				  .HasConversion(
					  vo => vo.Value,
					  str => Email.Create(str))
				  .IsRequired()
				  .HasMaxLength(255);

				// ClothingSize как owned внутри Employee
				eb.OwnsOne(e => e.ClothingSize, cb =>
				{
					cb.WithOwner();

					cb.Property(c => c.Id)
					  .HasColumnName("clothing_size_id")
					  .IsRequired();

					cb.Property(c => c.Name)
					  .HasColumnName("clothing_size_name")
					  .HasMaxLength(10)
					  .IsRequired();

					cb.Property(c => c.Description)
					  .HasColumnName("clothing_size_description")
					  .HasMaxLength(100)
					  .IsRequired(false);

					// Говорим EF: использовать свойства, а не конструктор
					cb.UsePropertyAccessMode(PropertyAccessMode.PreferProperty);
				});

				eb.Navigation(e => e.ClothingSize)
				  .UsePropertyAccessMode(PropertyAccessMode.PreferProperty);
			});
		}
	}
}



#region old
//builder.ToTable("merchandise_requests");
//builder.HasKey(x => x.Id);
//builder.Property(x => x.Id).ValueGeneratedOnAdd();

//// Shadow property для внешнего ключа
//builder.Property<long>("SkuPresetId")
//	.HasColumnName("sku_preset_id")
//	.IsRequired();

//builder.HasOne(x => x.SkuPreset)
//	.WithMany()
//	.HasForeignKey("SkuPresetId")
//	.OnDelete(DeleteBehavior.Cascade);

//// Employee (разложен на примитивы)
//builder.Property<string>("EmployeeEmail")
//	.HasColumnName("employee_email")
//	.IsRequired();

//builder.Property<string>("ClothingSize")
//	.HasColumnName("clothing_size");

//builder.Ignore(x => x.Employee);

//// Статус заявки
//builder.Property(x => x.Status)
//	.HasConversion(
//		x => x.Id,
//		x => MerchandiseRequestStatus
//		.GetAll<MerchandiseRequestStatus>()
//		.First(e => e.Id == x))
//	.HasColumnName("status_id");

//// Даты
//builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
//builder.Property(x => x.GiveOutAt).HasColumnName("give_out_at").HasDefaultValue(DateTimeOffset.UtcNow);
#endregion
