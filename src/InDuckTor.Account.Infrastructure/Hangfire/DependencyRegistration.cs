using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Account.Infrastructure.Hangfire;

public static class DependencyRegistration
{
    public static IServiceCollection AddAccountsHangfire(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddHangfire(hangfireConfiguration =>
        {
            hangfireConfiguration.SetDataCompatibilityLevel(CompatibilityLevel.Version_180)
                .UseSimpleAssemblyNameTypeSerializer()
                .UseRecommendedSerializerSettings()
                .UsePostgreSqlStorage(options => { options.UseNpgsqlConnection(configuration.GetConnectionString("AccountDatabase")); });
        });
        // Add the processing server as IHostedService
        services.AddHangfireServer();

        return services;
    }
}