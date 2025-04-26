namespace Infrastructure.Extensions;

public static class ConfigurationManagerExtensions
{
    /// <summary>
    /// <see cref="Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource.Prefix"/>
    /// </summary>
    public static readonly string EnvironmentVariablePrefix = "FORGAMERS__";

    /// <summary>
    /// Prepares all necessary configuration for the application.
    /// </summary>
    /// <param name="configurationManager">Current configuration</param>
    /// <param name="hostEnvironment"><see cref="IWebHostEnvironment"/></param>
    public static void Prepare(this ConfigurationManager configurationManager)
        => configurationManager.AddEnvironmentVariables();

    /// Adds ENV vars with specific prefix <see cref="EnvironmentVariablePrefix"/>.
    /// Those ENV vars override existing options/settings in loaded appsettings.*.json file
    /// </summary>
    /// <param name="configurationManager">Current configuration</param>
    /// <returns>Modified configuration with ENV vars</returns>
    private static IConfigurationBuilder AddEnvironmentVariables(this ConfigurationManager configurationManager)
        => configurationManager.AddEnvironmentVariables(ev => ev.Prefix = EnvironmentVariablePrefix);
}
