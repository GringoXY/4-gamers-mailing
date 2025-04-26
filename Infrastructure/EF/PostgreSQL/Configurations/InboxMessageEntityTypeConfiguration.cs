using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Shared.Entities;

namespace Infrastructure.EF.PostgreSQL.Configurations;

public class InboxMessageEntityTypeConfiguration : BaseEntityTypeConfiguration<InboxMessage>
{
    public override void Configure(EntityTypeBuilder<InboxMessage> builder)
    {
        builder
            .Property(om => om.EntityId)
            .IsRequired();

        builder
            .Property(d => d.EntityType)
            .HasColumnType("smallint")
            .IsRequired();

        builder
            .Property(om => om.EventType)
            .HasColumnType("varchar(255)")
            .IsRequired();

        builder
            .Property(om => om.Payload)
            .HasColumnType("text")
            .IsRequired();

        builder
            .Property(om => om.ProcessedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValue(null);

        base.Configure(builder);

        builder.HasIndex(om => om.CreatedAt);

        builder.Ignore(om => om.Remarks);
        builder.Ignore(om => om.Description);
        builder.Ignore(om => om.IsProcessed);
    }
}
