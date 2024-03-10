using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Account.Infrastructure.Database;

public record DatabaseSettings(string Scheme);

public static class DependencyRegistration
{
    public static IServiceCollection AddAccountsDbContext(this IServiceCollection serviceCollection, IConfiguration configuration)
    {
        var configurationSection = configuration.GetSection(nameof(DatabaseSettings));
        serviceCollection.Configure<DatabaseSettings>(configurationSection);
        var databaseSettings = configurationSection.Get<DatabaseSettings>();
        ArgumentNullException.ThrowIfNull(databaseSettings, nameof(configuration));

        return serviceCollection.AddDbContext<AccountsDbContext>(optionsBuilder =>
        {
            ((IDbContextOptionsBuilderInfrastructure)optionsBuilder)
                .AddOrUpdateExtension(new AccountsDbContextOptionsExtension(databaseSettings.Scheme));

            optionsBuilder
                .UseNpgsql(configuration.GetConnectionString("AccountDatabase"));
        });
    }
}

internal class AccountsDbContextOptionsExtension : IDbContextOptionsExtension
{
    public string? Schema { get; private set; }
    private DbContextOptionsExtensionInfo? _info;

    public AccountsDbContextOptionsExtension(string? schema = null)
    {
        Schema = schema;
    }

    public void ApplyServices(IServiceCollection services)
    {
    }

    public void Validate(IDbContextOptions options)
    {
    }

    public DbContextOptionsExtensionInfo Info => _info ??= new AccountsDbContextOptionsExtensionInfo(this);
}

internal class AccountsDbContextOptionsExtensionInfo : DbContextOptionsExtensionInfo
{
    public AccountsDbContextOptionsExtensionInfo(IDbContextOptionsExtension extension) : base(extension)
    {
    }

    public override int GetServiceProviderHashCode() => 0;

    public override bool ShouldUseSameServiceProvider(DbContextOptionsExtensionInfo other) => other is AccountsDbContextOptionsExtensionInfo;

    public override void PopulateDebugInfo(IDictionary<string, string> debugInfo)
    {
    }

    public override bool IsDatabaseProvider => false;
    public override string LogFragment => string.Empty;
}