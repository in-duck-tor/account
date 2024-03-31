using Hangfire;
using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.WebApi.BackgroundJobs;

public static class PullCbCurrencyJob
{
    public static void Schedule(IConfiguration configuration)
    {
        RecurringJob.AddOrUpdate(
            "PullCbCurrencyJob",
            // todo : think what todo with  cancellation tokens 
            (IExecutor<IPullCbCurrencyRoutine, Unit, Unit> routine) => routine.Execute(new Unit(), default),
            () => configuration["BackgroundJobs:PullCbCurrencyJob:CronExpression"],
            new RecurringJobOptions { TimeZone = TimeZoneInfo.Utc });
    }
}