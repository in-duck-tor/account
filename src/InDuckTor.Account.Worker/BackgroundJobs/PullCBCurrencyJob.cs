using Hangfire;
using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Worker.BackgroundJobs;

public static class PullCbCurrencyJob
{
    public static void Schedule(IConfiguration configuration)
    {
        RecurringJob.AddOrUpdate(
            "PullCbrCurrencyJob",
            // todo : think what todo with  cancellation tokens 
            (IExecutor<IPullCbrCurrencyRoutine, Unit, Unit> routine) => routine.Execute(new Unit(), default),
            () => configuration["BackgroundJobs:PullCbrCurrencyJob:CronExpression"],
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}