using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

using OzonEdu.MerchandiseService.DataAccess.EntityFramework.Models;

namespace OzonEdu.MerchandiseService.DataAccess.EntityFramework.DbContexts.ModelsConfigurations
{
    internal sealed class IntegrationInboxMessageConfiguration : IEntityTypeConfiguration<IntegrationInboxMessage>
    {
        public void Configure(EntityTypeBuilder<IntegrationInboxMessage> builder)
        {
            builder.ToTable("integration_inbox_messages");
            builder.HasKey(x => x.Id);

            builder.Property(x => x.Id).HasColumnName("id").HasMaxLength(255);
            builder.Property(x => x.Topic).HasColumnName("topic").HasMaxLength(255).IsRequired();
            builder.Property(x => x.Type).HasColumnName("message_type").HasMaxLength(512).IsRequired();
            builder.Property(x => x.ReceivedOnUtc).HasColumnName("received_on_utc").IsRequired();
            builder.Property(x => x.ProcessedOnUtc).HasColumnName("processed_on_utc");

            builder.HasIndex(x => x.ProcessedOnUtc);
        }
    }
}
