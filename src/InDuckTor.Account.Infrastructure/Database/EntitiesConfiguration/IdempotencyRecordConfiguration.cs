using InDuckTor.Shared.Idempotency.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InDuckTor.Account.Infrastructure.Database.EntitiesConfiguration;

public class IdempotencyRecordConfiguration : IEntityTypeConfiguration<IdempotencyRecord>
{
    public void Configure(EntityTypeBuilder<IdempotencyRecord> builder)
    {
        builder.ToTable(nameof(IdempotencyRecord));
        builder.HasKey(x => x.Key);

        builder.OwnsOne(x => x.CachedResponse, ownsBuilder =>
        {
            ownsBuilder.Property(x => x.Headers)
                .HasColumnType("jsonb");
        });
    }
}