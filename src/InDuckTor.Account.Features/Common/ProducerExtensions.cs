using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Shared.Kafka;

namespace InDuckTor.Account.Features.Common;

public interface ITransactionEventsProducer
{
    Task ProduceTransactionStarted(Transaction transaction, CancellationToken ct);
    Task ProduceTransactionFinished(Transaction transaction, CancellationToken ct);
}

public class TransactionEventsTopicProducer : ITransactionEventsProducer
{
    private readonly ITopicProducer<long, TransactionEnvelop> _producer;

    public TransactionEventsTopicProducer(ITopicProducer<long, TransactionEnvelop> producer)
    {
        _producer = producer;
    }

    public Task ProduceTransactionStarted(Transaction transaction, CancellationToken ct)
    {
        return _producer.Produce(CreateTransactionPartitionKey(transaction), transaction.ToStartedEventEnvelop(), ct);
    }

    public Task ProduceTransactionFinished(Transaction transaction, CancellationToken ct)
    {
        return _producer.Produce(CreateTransactionPartitionKey(transaction), transaction.ToFinishedEventEnvelop(), ct);
    }

    private static long CreateTransactionPartitionKey(Transaction transaction)
    {
        var withdrawFromCode = transaction.WithdrawFrom?.AccountNumber.Value.GetHashCode() ?? 0;
        var depositOnCode = transaction.DepositOn?.AccountNumber.Value.GetHashCode() ?? 0;

        Span<byte> bytes = stackalloc byte[sizeof(long)];

        if (withdrawFromCode > depositOnCode)
        {
            BitConverter.TryWriteBytes(bytes, withdrawFromCode);
            BitConverter.TryWriteBytes(bytes[sizeof(int) ..], depositOnCode);
        }
        else
        {
            BitConverter.TryWriteBytes(bytes, depositOnCode);
            BitConverter.TryWriteBytes(bytes[sizeof(int) ..], withdrawFromCode);
        }

        return BitConverter.ToInt64(bytes);
    }
}