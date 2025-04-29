using Infrastructure.EF.PostgreSQL.Repositories;
using Infrastructure.EF.PostgreSQL;
using Infrastructure.Options;
using Shared.Repositories;
using Infrastructure.BackgroundServices;
using Shared.Apis;
using Infrastructure.Apis;

namespace Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection Configure(this IServiceCollection services, ConfigurationManager configuration)
    {
        services.LoadSettings(configuration);
        services.AddLogging();
        services.AddAutoMapper();
        services.AddHttpClient();
        services.AddApis();
        services.AddHostedServices();
        services.AddEntityFramework();

        return services;
    }

    private static void LoadSettings(this IServiceCollection services, ConfigurationManager configuration)
        => services
            .Configure<OutboxMessagesConsumerOptions>(configuration.GetSection(OutboxMessagesConsumerOptions.SectionName))
            .Configure<SendEmailInboxMessagesOptions>(configuration.GetSection(SendEmailInboxMessagesOptions.SectionName))
            .Configure<DocsApiOptions>(configuration.GetSection(DocsApiOptions.SectionName))
            .Configure<PostgreSQLOptions>(configuration.GetSection(PostgreSQLOptions.SectionName));

    private static IServiceCollection AddHostedServices(this IServiceCollection services)
        => services.AddHostedService<OutboxMessagesConsumerBackgroundService>()
                   .AddHostedService<SendEmailInboxMessagesBackgroundService>();

    /// <summary>
    /// Adds auto mapper profiles
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Configured auto mapper's profiles</returns>
    private static IServiceCollection AddAutoMapper(this IServiceCollection services)
        => services.AddAutoMapper(Assembly.Info);

    /// <summary>
    /// Adds infrastructure
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Modified services collection by infrastructure</returns>
    public static IServiceCollection AddEntityFramework(this IServiceCollection services)
        => services.AddPostgreSQL();

    /// <summary>
    /// Sets up everything related to PostgreSQL
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Modified services collection by PostgreSQL setup</returns>
    private static IServiceCollection AddPostgreSQL(this IServiceCollection services)
        => services.AddDbContext()
                   .AddRepositories()
                   .AddUnitOfWork();

    /// <summary>
    /// Sets up DbContext for PostgreSQL
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Modified services collection by PostgreSQL setup</returns>
    private static IServiceCollection AddDbContext(this IServiceCollection services)
        => services.AddDbContext<ApplicationDbContext>();

    /// <summary>
    /// Sets up repositories for PostgreSQL
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Modified services collection by repositories setup</returns>
    private static IServiceCollection AddRepositories(this IServiceCollection services)
        => services.AddTransient<IInboxMessageRepository, InboxMessageRepository>();

    /// <summary>
    /// Sets up <see cref="UnitOfWork"/>
    /// </summary>
    /// <param name="services">Specifies the contract for a collection of service descriptors</param>
    /// <returns>Modified services collection by <see cref="UnitOfWork"/></returns>
    private static IServiceCollection AddUnitOfWork(this IServiceCollection services)
        => services.AddTransient<IUnitOfWork, UnitOfWork>();

    private static IServiceCollection AddApis(this IServiceCollection services)
        => services.AddTransient<IDocsApi, DocsApi>();
}
