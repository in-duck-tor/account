using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;

namespace InDuckTor.Account.KafkaClient;

public static class ProducerExtensions
{
    public static Task ProduceAccountCommand(this ITopicProducer<AccountCommandKey, AccountCommandEnvelop> producer, AccountCommandEnvelop envelop, Headers? headers = null, CancellationToken cancellationToken = default)
    {
        return producer.Produce(new AccountCommandKey(envelop), envelop, headers, cancellationToken);
    }
}