using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts.ModelsConfigurations
{
    internal sealed class IntegrationOutboxMessageConfiguration : IEntityTypeConfiguration<IntegrationOutboxMessage>
    {
        public void Configure(EntityTypeBuilder<IntegrationOutboxMessage> builder)
        {
            builder.ToTable("integration_outbox_messages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id");
            builder.Property(x => x.Topic).HasColumnName("topic").HasMaxLength(255).IsRequired();
            builder.Property(x => x.Key).HasColumnName("message_key").HasMaxLength(255).IsRequired();
            builder.Property(x => x.Type).HasColumnName("message_type").HasMaxLength(512).IsRequired();
            builder.Property(x => x.Payload).HasColumnName("payload").IsRequired();
            builder.Property(x => x.OccurredOnUtc).HasColumnName("occurred_on_utc").IsRequired();
            builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc");
            builder.Property(x => x.Error).HasColumnName("error");

            builder.HasIndex(x => x.ProcessedOnUtc);
            builder.HasIndex(x => new { x.Topic, x.OccurredOnUtc });
        }
    }
}
