using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class BankInfoConfiguration : IEntityTypeConfiguration<BankInfo>
{
    public void Configure(EntityTypeBuilder<BankInfo> builder)
    {
        builder.ToTable(nameof(BankInfo)).HasKey(x => x.BankCode);
    }
}