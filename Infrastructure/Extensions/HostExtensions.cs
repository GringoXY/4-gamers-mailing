using Infrastructure.EF.PostgreSQL;
using Microsoft.EntityFrameworkCore;

namespace Infrastructure.Extensions;

public static class HostExtension
{
    public static IHost Configure(this IHost host)
        => host.ApplyMigrations();

    private static IHost ApplyMigrations(this IHost host)
    {
        using var scope = host.Services.CreateScope();
        var services = scope.ServiceProvider;

        var context = services.GetRequiredService<ApplicationDbContext>();
        context.Database.Migrate();

        return host;
    }
}
