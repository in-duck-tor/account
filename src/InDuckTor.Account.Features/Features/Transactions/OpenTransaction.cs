using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Transactions;

public class OpenTransactionRequest
{
    public required NewTransactionRequest NewTransaction { get; set; }
    public bool ExecuteImmediate { get; set; } = false;
    public TimeSpan? RequestedTransactionTtl { get; set; }
}

public record OpenTransactionResult(long TransactionId, TransactionType TransactionType, TransactionStatus Status, DateTime AutoCloseAt);

public interface IOpenTransaction : ICommand<OpenTransactionRequest, Result<OpenTransactionResult>>;

public class OpenTransaction(
    AccountsDbContext context,
    IExecutor<ICreateTransaction, CreateTransactionParams, Result<Transaction>> createTransaction,
    IExecutor<ICommitTransaction, long, Result> commitTransaction)
    : IOpenTransaction
{
    public async Task<Result<OpenTransactionResult>> Execute(OpenTransactionRequest input, CancellationToken ct)
    {
        var result = await createTransaction.Execute(new CreateTransactionParams(input.NewTransaction, input.RequestedTransactionTtl), ct);
        if (!result.IsSuccess) return result.ToResult();

        var transaction = result.Value;
        if (input.ExecuteImmediate)
        {
            await ScheduleTransactionRoutine(transaction, ct);
        }

        return new OpenTransactionResult(transaction.Id, transaction.Type, transaction.Status, transaction.AutoCloseAt);
    }

    // todo use hangfire
    private async Task ScheduleTransactionRoutine(Transaction transaction, CancellationToken ct)
    {
        await commitTransaction.Execute(transaction.Id, ct);
    }
}