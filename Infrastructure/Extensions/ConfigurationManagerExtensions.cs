namespace Infrastructure.Extensions;

public static class ConfigurationManagerExtensions
{
    /// <summary>
    /// <see cref="Microsoft.Extensions.Configuration.EnvironmentVariables.EnvironmentVariablesConfigurationSource.Prefix"/>
    /// </summary>
    public static readonly string EnvironmentVariablePrefix = "FORGAMERS__";

    /// <summary>
    /// The relative path to appsettings.*.json files (where configuration files are being stored)
    /// </summary>
    public static readonly string AppSettingsPath = "appsettings";

    /// <summary>
    /// The name of the environment variable that has a value 
    /// which informs about app's environment
    /// </summary>
    public static readonly string DotnetEnvironmentVariableName = "DOTNET_ENVIRONMENT";

    /// <summary>
    /// Prepares all necessary configuration for the application.
    /// </summary>
    /// <param name="configurationManager">Current configuration</param>
    /// <param name="hostEnvironment"><see cref="IWebHostEnvironment"/></param>
    public static void Prepare(this ConfigurationManager configurationManager)
        => configurationManager.AddAppSettings()
                               .AddEnvironmentVariables();

    /// <summary>
    /// Add specific JSON file which contains settings for specific environment
    /// </summary>
    /// <param name="configurationManager">Current configuration</param>
    /// <param name="hostEnvironment"><see cref="IWebHostEnvironment"/></param>
    /// <returns>Modified configuration with JSON file</returns>
    private static ConfigurationManager AddAppSettings(this ConfigurationManager configurationManager)
    {
        // Retrieve environment name from configuration or environment variable
        var environmentName = configurationManager[DotnetEnvironmentVariableName]
            ?? Environment.GetEnvironmentVariable(DotnetEnvironmentVariableName)
            ?? "Production";

        configurationManager.AddJsonFile($"{AppSettingsPath}{Path.DirectorySeparatorChar}appsettings.{environmentName}.json");
        return configurationManager;
    }

    /// Adds ENV vars with specific prefix <see cref="EnvironmentVariablePrefix"/>.
    /// Those ENV vars override existing options/settings in loaded appsettings.*.json file
    /// </summary>
    /// <param name="configurationManager">Current configuration</param>
    /// <returns>Modified configuration with ENV vars</returns>
    private static IConfigurationBuilder AddEnvironmentVariables(this ConfigurationManager configurationManager)
        => configurationManager.AddEnvironmentVariables(ev => ev.Prefix = EnvironmentVariablePrefix);
}
