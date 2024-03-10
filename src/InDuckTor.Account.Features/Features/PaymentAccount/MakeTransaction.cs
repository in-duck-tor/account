using FluentResults;
using FluentResults.Extensions;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

public interface IMakeTransaction : ICommand<NewTransactionRequest, Result<IdResult<long>>>;

public class MakeTransaction(
    AccountsDbContext context,
    IExecutor<ICreateTransaction, CreateTransactionParams, Result<Transaction>> createTransaction,
    IExecutor<ICommitTransaction, long, Result> commitTransaction)
    : IMakeTransaction
{
    public Task<Result<IdResult<long>>> Execute(NewTransactionRequest input, CancellationToken ct)
        => createTransaction.Execute(new CreateTransactionParams(input) , ct)
            .Bind(transaction => EnsureTransactionAccountTypes(transaction, ct))
            .Bind(async transaction =>
            {
                await ScheduleTransactionRoutine(transaction, ct);
                await context.SaveChangesAsync(ct);
                return Result.Ok(new IdResult<long>(transaction.Id));
            });

    private async Task<Result<Transaction>> EnsureTransactionAccountTypes(Transaction transaction, CancellationToken ct) 
        => await EnsureAccountType(transaction.DepositOn, ct)
           && await EnsureAccountType(transaction.WithdrawFrom, ct)
            ? transaction
            : new Errors.Forbidden();

    private async ValueTask<bool> EnsureAccountType(TransactionTarget? target, CancellationToken ct)
        => target is null
           || target.InExternal
           || (await context.Accounts.FindAsync([ target.AccountNumber ], ct))?.Type == AccountType.Payment;

    // todo use hangfire
    private async Task ScheduleTransactionRoutine(Transaction transaction, CancellationToken ct)
    {
        await commitTransaction.Execute(transaction.Id, ct);
    }
}