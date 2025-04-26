using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using Infrastructure.Options;
using Shared.Entities;

namespace Infrastructure.EF.PostgreSQL;

public sealed class ApplicationDbContext(
    DbContextOptions<ApplicationDbContext> options,
    IOptions<PostgreSQLOptions> postgreSQLOptions) : DbContext(options)
{
    private readonly PostgreSQLOptions _postgreSQLOptions = postgreSQLOptions.Value;

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        optionsBuilder
            .UseNpgsql(_postgreSQLOptions.ConnectionString)
            .UseSnakeCaseNamingConvention();

        base.OnConfiguring(optionsBuilder);
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);

        // Fixes exception:
        // """
        // Microsoft.EntityFrameworkCore.DbUpdateException: An error occurred while saving the entity changes.
        // See the inner exception for details. --->
        // System.ArgumentException: Cannot write DateTime with Kind=Unspecified to PostgreSQL type 'timestamp with time zone',
        // only UTC is supported. Note that it's not possible to mix DateTimes with different Kinds in an array, range, or multirange. (Parameter 'value')
        // """
        var dateTimeConverter = new ValueConverter<DateTime, DateTime>(
            v => v.Kind == DateTimeKind.Utc ? v : DateTime.SpecifyKind(v, DateTimeKind.Utc),
            v => DateTime.SpecifyKind(v, DateTimeKind.Utc));

        foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            foreach (
                var property in entityType
                    .GetProperties()
                    .Where(p => p.ClrType == typeof(DateTime) || p.ClrType == typeof(DateTime?)))
            {
                property.SetValueConverter(dateTimeConverter);
            }
        }

        base.OnModelCreating(modelBuilder);
    }

    public DbSet<InboxMessage> InboxMessages { get; set; }
}
