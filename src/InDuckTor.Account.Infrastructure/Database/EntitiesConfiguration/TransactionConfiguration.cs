using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable(nameof(Transaction)).HasKey(x => x.Id);

        builder.OwnsOne(x => x.DepositOn, ConfigureTarget);
        builder.OwnsOne(x => x.WithdrawFrom, ConfigureTarget);
    }

    private void ConfigureTarget(OwnedNavigationBuilder<Transaction, TransactionTarget> ownsBuilder)
    {
        ownsBuilder.HasIndex(x => x.AccountNumber);
        ownsBuilder.HasOne(x => x.Account)
            .WithMany()
            .HasForeignKey(x => x.AccountNumber)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.NoAction);

        ownsBuilder.HasOne(x => x.BankInfo).WithMany().HasForeignKey(x => x.BankCode);
        ownsBuilder.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyCode);
        ownsBuilder.Navigation(x => x.Currency).AutoInclude();
        
        ownsBuilder
            .Ignore(x => x.Money)
            .Ignore(x => x.IsExternal);
    }
}