using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Strategies;
using TransactionStatus = InDuckTor.Account.Domain.TransactionStatus;
using TransactionType = InDuckTor.Account.Domain.TransactionType;

namespace InDuckTor.Account.Features.Transactions;

public class OpenTransactionRequest
{
    public required NewTransactionRequest NewTransaction { get; set; }

    /// <summary>
    /// Выполнить трансакцию сразу при получении запроса или ожидать команды на завершение
    /// </summary>
    public bool ExecuteImmediate { get; set; } = false;

    /// <summary>
    /// Запрашиваемое максимальное время жизни трансакции в секундах 
    /// </summary>
    public double? RequestedTransactionTtl { get; set; }
}

public record OpenTransactionResult(long TransactionId, TransactionType TransactionType, TransactionStatus Status, DateTime AutoCloseAt);

public interface IOpenTransaction : ICommand<OpenTransactionRequest, Result<OpenTransactionResult>>;

public class OpenTransaction(
    AccountsDbContext context,
    IExecutor<ICreateTransaction, CreateTransactionParams, Result<Transaction>> createTransaction,
    IExecutor<ICommitTransaction, long, Result> commitTransaction,
    ITransactionEventsProducer producer)
    : IOpenTransaction
{
    public async Task<Result<OpenTransactionResult>> Execute(OpenTransactionRequest input, CancellationToken ct)
    {
        TimeSpan? ttl = input.RequestedTransactionTtl != null 
            ? TimeSpan.FromSeconds(input.RequestedTransactionTtl.Value) 
            : null;
        
        var result = await createTransaction.Execute(new CreateTransactionParams(input.NewTransaction, ttl), ct);
        if (!result.IsSuccess) return result.ToResult();

        await context.SaveChangesAsync(ct);

        var transaction = result.Value;
        await producer.ProduceTransactionStarted(transaction, ct);
        
        if (input.ExecuteImmediate)
        {
            await EvaluateTransaction(transaction, ct);
        }
        
        return new OpenTransactionResult(transaction.Id, transaction.Type, transaction.Status, transaction.AutoCloseAt);
    }

    private async Task EvaluateTransaction(Transaction transaction, CancellationToken ct)
    {
        // todo create non-holistic strategy and reuse it
        await commitTransaction.Execute(transaction.Id, ct);
    }
}