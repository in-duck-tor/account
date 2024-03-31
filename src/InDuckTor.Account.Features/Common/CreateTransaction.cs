using FluentResults;
using FluentResults.Extensions;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.Utils;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Common;

public record struct CreateTransactionParams(NewTransactionRequest NewTransactionRequest, TimeSpan? TransactionTtl = null);

public interface ICreateTransaction : ICommand<CreateTransactionParams, Result<Transaction>>;

// todo add ForUpdates
public class CreateTransaction(AccountsDbContext context, ISecurityContext securityContext) : ICreateTransaction
{
    public async Task<Result<Transaction>> Execute(CreateTransactionParams input, CancellationToken ct)
        => await CreateNew(input.NewTransactionRequest, input.TransactionTtl ?? Transaction.DefaultTtl, ct)
            .Bind(transaction => EnsureAccountAmount(transaction, ct))
            .Bind(AddFundsReservation)
            .Map(transaction =>
            {
                transaction.Status = TransactionStatus.Pending;
                context.Transactions.Add(transaction);
                return transaction;
            });


    private static readonly Error Forbid = new Errors.Forbidden();

    private async ValueTask<Result<Transaction>> CreateNew(NewTransactionRequest request, TimeSpan ttl, CancellationToken ct)
    {
        if (request is
            {
                WithdrawFrom.BankCode.IsExternal: true, 
                DepositOn.BankCode.IsExternal: true
            })
            return new Errors.InvalidInput("Необходимо указать известный счёт отправитель или счёт получатель");

        var currantUser = securityContext.Currant;

        var withdrawFromResult = await TryCreateCreate(request.Amount, request.WithdrawFrom,
            account => Result.OkIf(account.CanUserWithdraw(currantUser), Forbid),
            ct);

        var depositOnResult = await TryCreateCreate(request.Amount, request.DepositOn,
            account => Result.OkIf(account.CanUserDeposit(currantUser), Forbid),
            ct);

        return withdrawFromResult.Merge(depositOnResult)
            .Bind(Result<Transaction> (targets) =>
            {
                var (withdrawFrom, depositOn) = targets;
                if (withdrawFrom != null
                    && depositOn != null
                    && withdrawFrom.CurrencyCode != depositOn.CurrencyCode)
                    // todo
                    return new Errors.Conflict("Переводы с разными валютами не реализованы");

                return new Transaction(depositOn, withdrawFrom, ttl, currantUser.Id);
            });
    }

    private async ValueTask<Result<TransactionTarget?>> TryCreateCreate(decimal amount, NewTransactionRequest.Target? requestTarget, Func<Domain.Account, Result> checkAccount, CancellationToken ct)
    {
        if (requestTarget is null) return Result.Ok<TransactionTarget?>(null);

        if (requestTarget.BankCode != Domain.BankInfo.InDuckTorBankCode)
            return new TransactionTarget(amount, requestTarget.AccountNumber, null, requestTarget.BankCode);

        var account = await context.Accounts.FindAsync([ requestTarget.AccountNumber ], ct);
        if (account is null) return new DomainErrors.Account.NotFound(requestTarget.AccountNumber);

        var result = checkAccount(account);
        if (result.IsFailed) return result;

        return new TransactionTarget(amount, account.Number, account.CurrencyCode, account.BankCode);
    }


    private async ValueTask<Result<Transaction>> EnsureAccountAmount(Transaction transaction, CancellationToken ct)
    {
        if (transaction is not { WithdrawFrom.IsExternal: false }) return transaction;

        var account = await context.Accounts.FindAsync([ transaction.WithdrawFrom.AccountNumber ], ct);
        var fundsReservations = await context.FundsReservations
            .Where(x => x.AccountNumber == account!.Number)
            .ToListAsync(ct);

        var reserved = fundsReservations.Aggregate(new decimal(), (sum, reservation) => sum + reservation.Amount);
        var freeFunds = account!.Amount - reserved;

        return freeFunds >= transaction.WithdrawFrom.Amount
            ? transaction
            : new DomainErrors.Account.NotEnoughFunds();
    }

    private Result<Transaction> AddFundsReservation(Transaction transaction)
    {
        if (transaction is not { WithdrawFrom.IsExternal: false }) return transaction;

        var fundsReservation = new FundsReservation
        {
            Amount = transaction.WithdrawFrom.Amount,
            AccountNumber = transaction.WithdrawFrom.AccountNumber
        };
        context.Add(fundsReservation);
        return transaction.AddReservations(fundsReservation);
    }
}