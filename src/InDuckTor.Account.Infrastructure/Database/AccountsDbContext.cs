using System.Reflection;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InDuckTor.Account.Infrastructure.Database;

public class AccountsDbContext : DbContext
{
    public string? Schema { get; }

    public AccountsDbContext(DbContextOptions<AccountsDbContext> dbContextOptions) : base(dbContextOptions)
    {
        Schema = dbContextOptions.Extensions.OfType<AccountsDbContextOptionsExtension>()
            .FirstOrDefault()
            ?.Schema;
    }

    public virtual DbSet<Domain.Account> Accounts { get; set; } = null!;
    public virtual DbSet<BankInfo> Banks { get; set; } = null!;
    public virtual DbSet<Currency> Currencies { get; set; } = null!;
    public virtual DbSet<FundsReservation> FundsReservations { get; set; } = null!;
    public virtual DbSet<Transaction> Transactions { get; set; } = null!;

    public const string AccountPersonalCodeSequenceName = "account_personal_code_seq";

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.HasDefaultSchema(Schema);
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        modelBuilder.HasSequence<long>(AccountPersonalCodeSequenceName);
    }

    protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
    {
        base.ConfigureConventions(configurationBuilder);
        configurationBuilder.Properties<AccountNumber>().HaveConversion<AccountNumberConverter>();
        configurationBuilder.Properties<BankCode>().HaveConversion<BankCodeConverter>();
    }
}