namespace Infrastructure.Extensions;

public static class EnvironmentExtensions
{
    public static bool IsMigratable(this IHostEnvironment hostEnvironment)
    {
        if (hostEnvironment is null)
        {
            throw new ArgumentNullException(nameof(hostEnvironment));
        }

        return hostEnvironment.IsDevelopment();
    }
}
