using FluentResults;
using FluentResults.Extensions;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

public record MakeTransactionResult(long TransactionId, TransactionType TransactionType, TransactionStatus Status);

public interface IMakeTransaction : ICommand<NewTransactionRequest, Result<MakeTransactionResult>>;

public class MakeTransaction(
    AccountsDbContext context,
    IExecutor<ICreateTransaction, CreateTransactionParams, Result<Transaction>> createTransaction,
    IExecutor<ICommitTransaction, long, Result> commitTransaction)
    : IMakeTransaction
{
    public Task<Result<MakeTransactionResult>> Execute(NewTransactionRequest input, CancellationToken ct)
        => createTransaction.Execute(new CreateTransactionParams(input), ct)
            .Bind(transaction => EnsureTransactionAccountTypes(transaction, ct))
            .Bind(async transaction =>
            {
                await context.SaveChangesAsync(ct);
                await ScheduleTransactionRoutine(transaction, ct);
                return Result.Ok(new MakeTransactionResult(transaction.Id, transaction.Type, transaction.Status));
            });

    private async Task<Result<Transaction>> EnsureTransactionAccountTypes(Transaction transaction, CancellationToken ct)
        => await EnsureAccountType(transaction.DepositOn, ct)
           && await EnsureAccountType(transaction.WithdrawFrom, ct)
            ? transaction
            : new Errors.Forbidden();

    private async ValueTask<bool> EnsureAccountType(TransactionTarget? target, CancellationToken ct)
        => target is null
           || target.IsExternal
           || (await context.Accounts.FindAsync([ target.AccountNumber ], ct))?.Type == AccountType.Payment;

    // todo use hangfire
    private async Task ScheduleTransactionRoutine(Transaction transaction, CancellationToken ct)
    {
        await commitTransaction.Execute(transaction.Id, ct);
    }
}