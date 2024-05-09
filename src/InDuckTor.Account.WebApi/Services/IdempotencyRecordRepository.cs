using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Idempotency.Http;
using Microsoft.EntityFrameworkCore;
using Npgsql;

namespace InDuckTor.Account.WebApi.Services;

public class IdempotencyRecordRepository : IIdempotencyRecordRepository
{
    private readonly AccountsDbContext _dbContext;

    public IdempotencyRecordRepository(AccountsDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public ValueTask<IdempotencyRecord?> Get(string key, CancellationToken cancellationToken)
    {
        return _dbContext.Set<IdempotencyRecord>().FindAsync([ key ], cancellationToken);
    }

    public async ValueTask<IIdempotencyRecordRepository.AddResult> Add(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        try
        {
            _dbContext.Add(record);
            await _dbContext.SaveChangesAsync(cancellationToken);
            return IIdempotencyRecordRepository.AddResult.Success;
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException { HResult: 23505 })
        {
            return IIdempotencyRecordRepository.AddResult.KeyViolation;
        }
    }

    public async ValueTask Set(IdempotencyRecord record, CancellationToken cancellationToken)
    {
        _dbContext.Update(record);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }
}