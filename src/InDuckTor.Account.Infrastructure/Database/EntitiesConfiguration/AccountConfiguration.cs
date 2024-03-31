using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class AccountConfiguration : IEntityTypeConfiguration<Domain.Account>
{
    public void Configure(EntityTypeBuilder<Domain.Account> builder)
    {
        builder.ToTable(nameof(Domain.Account)).HasKey(x => x.Number);

        builder.Property(x => x.Number).ValueGeneratedNever();
        builder.HasOne(x => x.Currency).WithMany().HasForeignKey(x => x.CurrencyCode);
        builder.HasOne(x => x.BankInfo).WithMany().HasForeignKey(x => x.BankCode);
        builder.Property(x => x.GrantedUsers).HasColumnType("jsonb");
    }
}