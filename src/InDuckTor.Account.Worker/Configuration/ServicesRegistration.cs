using InDuckTor.Account.Features.Common;

namespace InDuckTor.Account.Worker.Configuration;

public static class ServicesRegistration
{
    public static IServiceCollection AddWorkerServices(this IServiceCollection services)
    {
        return services.AddSingleton<ITransactionEventsProducer, TransactionEventsTopicProducer>();
    }  
}