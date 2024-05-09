using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Worker.BackgroundJobs;

namespace InDuckTor.Account.Worker.Configuration;

public static class ServicesRegistration
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        return services.AddSingleton<ITransactionEventsProducer, TransactionEventsTopicProducer>()
            .AddScoped<RemoveExpiredIdempotencyRecordsJob>();
    }  
}