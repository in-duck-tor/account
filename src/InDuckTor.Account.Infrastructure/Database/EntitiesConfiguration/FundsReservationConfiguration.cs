using InDuckTor.Account.Domain;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class FundsReservationConfiguration : IEntityTypeConfiguration<FundsReservation>
{
    public void Configure(EntityTypeBuilder<FundsReservation> builder)
    {
        builder.ToTable(nameof(FundsReservation)).HasKey(x => x.Id);

        builder.HasOne<Transaction>()
            .WithMany(transaction => transaction.Reservations)
            .HasForeignKey(x => x.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}