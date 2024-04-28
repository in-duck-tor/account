using InDuckTor.Account.Features.Common;

namespace InDuckTor.Account.WebApi.Configuration;

public static class ServicesRegistration
{
    public static IServiceCollection AddWebApiServices(this IServiceCollection services)
    {
        return services.AddSingleton<ITransactionEventsProducer, TransactionEventsTopicProducer>();
    }  
}