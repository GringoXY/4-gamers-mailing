using Shared.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Infrastructure.EF.PostgreSQL.Configurations;

public class BaseEntityTypeConfiguration<TEntity> : IEntityTypeConfiguration<TEntity> where TEntity : BaseEntity
{
    public virtual void Configure(EntityTypeBuilder<TEntity> builder)
    {
        builder.HasQueryFilter(e => e.DeletedAt == null);

        builder
            .HasKey(e => e.Id);

        builder
            .HasIndex(e => e.Id)
            .IsUnique()
            .HasMethod(PostgreSQLExtensions.BtreeIndexMethodName);

        builder
            .Property(e => e.CreatedAt)
            .HasColumnType("timestamp with time zone")
            .IsRequired();

        builder
            .Property(e => e.UpdatedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValue(null);

        builder
            .Property(e => e.DeletedAt)
            .HasColumnType("timestamp with time zone")
            .HasDefaultValue(null);

        builder
            .Ignore(e => e.IsDeleted);

        builder
            .Property(e => e.Remarks)
            .HasColumnType("varchar(1000)")
            .HasDefaultValue(string.Empty);

        builder
            .Property(e => e.Description)
            .HasColumnType("text")
            .HasDefaultValue(string.Empty);
    }
}
