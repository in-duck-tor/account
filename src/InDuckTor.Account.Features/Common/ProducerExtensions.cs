using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Shared.Kafka;

namespace InDuckTor.Account.Features.Common;

public static class ProducerExtensions
{
    public static Task ProduceTransactionStarted(this ITopicProducer<string, TransactionEnvelop> producer, Transaction transaction, CancellationToken ct)
    {
        return producer.Produce(CreateTransactionPartitionKey(transaction), transaction.ToStartedEventEnvelop(), ct);
    }
    
    public static Task ProduceTransactionFinished(this ITopicProducer<string, TransactionEnvelop> producer, Transaction transaction, CancellationToken ct)
    {
        return producer.Produce(CreateTransactionPartitionKey(transaction),transaction.ToFinishedEventEnvelop(),ct);
    }

    private static string CreateTransactionPartitionKey(Transaction transaction) => $"{transaction.WithdrawFrom?.AccountNumber}-{transaction.DepositOn?.AccountNumber}";
}