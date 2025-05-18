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
			builder.HasKey(x => x.Id);
			builder.Property(x => x.Id).ValueGeneratedOnAdd();

			// Shadow property для внешнего ключа
			builder.Property<long>("SkuPresetId")
				.HasColumnName("sku_preset_id")
				.IsRequired();

			builder.HasOne(x => x.SkuPreset)
				.WithMany()
				.HasForeignKey("SkuPresetId")
				.OnDelete(DeleteBehavior.Cascade);

			// Employee (разложен на примитивы)
			builder.Property<string>("EmployeeEmail")
				.HasColumnName("employee_email")
				.IsRequired();

			builder.Property<string>("ClothingSize")
				.HasColumnName("clothing_size");

			builder.Ignore(x => x.Employee);

			// Статус заявки
			builder.Property(x => x.Status)
				.HasConversion(
					x => x.Id,
					x => MerchandiseRequestStatus
					.GetAll<MerchandiseRequestStatus>()
					.First(e => e.Id == x))
				.HasColumnName("status_id");

			// Даты
			builder.Property(x => x.CreatedAt).HasColumnName("created_at").HasDefaultValueSql("CURRENT_TIMESTAMP");
			builder.Property(x => x.GiveOutAt).HasColumnName("give_out_at").HasDefaultValue(DateTimeOffset.UtcNow);
		}
	}
}
