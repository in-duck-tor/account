using Hangfire;
using Hangfire.Server;
using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Idempotency.Http;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Worker.BackgroundJobs;

public class RemoveExpiredIdempotencyRecordsJob
{
    private readonly AccountsDbContext _dbContext;
    private readonly ILogger<RemoveExpiredIdempotencyRecordsJob> _logger;

    public RemoveExpiredIdempotencyRecordsJob(AccountsDbContext dbContext, ILogger<RemoveExpiredIdempotencyRecordsJob> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task Execute(PerformContext context)
    {
        _logger.LogDebug("Выполняется чистка устаревших записей идемпотентности");
        var deletedCount = await _dbContext.Set<IdempotencyRecord>()
            .Where(x => x.ExpiresAt < DateTime.UtcNow)
            .ExecuteDeleteAsync(context.CancellationToken.ShutdownToken);
        _logger.LogInformation("Удалено {DeletedIdempotencyRecordCount} записей идемпотентности", deletedCount);
    }

    public static void Schedule(IConfiguration configuration)
    {
        RecurringJob.AddOrUpdate<RemoveExpiredIdempotencyRecordsJob>(
            "RemoveExpiredIdempotencyRecordsJob",
            job => job.Execute(null!),
            () => configuration["BackgroundJobs:RemoveExpiredIdempotencyRecordsJob:CronExpression"],
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}