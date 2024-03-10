using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable(nameof(Transaction)).HasKey(x => x.Id);

        builder.OwnsOne(x => x.DepositOn, ownsBuilder =>
        {
            ownsBuilder.HasIndex(x => x.AccountNumber);
            // ownsBuilder.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyCode);
            ownsBuilder.HasOne(x => x.BankInfo).WithMany().HasForeignKey(x => x.BankCode);
        });

        builder.OwnsOne(x => x.WithdrawFrom, ownsBuilder =>
        {
            ownsBuilder.HasIndex(x => x.AccountNumber);
            // ownsBuilder.HasOne<Currency>().WithMany().HasForeignKey(x => x.CurrencyCode);
            ownsBuilder.HasOne(x => x.BankInfo).WithMany().HasForeignKey(x => x.BankCode);
        });
    }
}