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
        var currantUser = securityContext.Currant;

        var withdrawAccountResult = await TryGetAccount(request.WithdrawFrom, account => Result.OkIf(account.CanUserWithdraw(currantUser), Forbid), ct);
        var depositAccountResult = await TryGetAccount(request.DepositOn, account => Result.OkIf(account.CanUserDeposit(currantUser), Forbid), ct);

        return withdrawAccountResult.Merge(depositAccountResult)
            .Bind(Result<Transaction> (accounts) =>
            {
                var (withdrawAccount, depositAccount) = accounts;
                if (withdrawAccount == null && depositAccount == null) return new Errors.InvalidInput("Необходимо указать известный счёт отправитель или счёт получатель");

                var targetCurrency = withdrawAccount?.Currency ?? depositAccount!.Currency;
                var transactionMoneyAmount = new Money(request.Amount, targetCurrency);

                return new Transaction(
                    depositOn: CreateTarget(transactionMoneyAmount, request.DepositOn, depositAccount),
                    withdrawFrom: CreateTarget(transactionMoneyAmount, request.WithdrawFrom, depositAccount),
                    ttl,
                    currantUser.Id);
            });
    }

    private async Task<Result<Domain.Account?>> TryGetAccount(NewTransactionRequest.Target? requestTarget, Func<Domain.Account, Result> checkAccount, CancellationToken ct)
    {
        if (requestTarget is not { BankCode.IsExternal: false }) return Result.Ok<Domain.Account?>(null);

        var account = await context.Accounts.FindAsync([ requestTarget.AccountNumber ], ct);
        if (account is null) return new DomainErrors.Account.NotFound(requestTarget.AccountNumber);

        var result = checkAccount(account);
        if (result.IsFailed) return result;

        return account;
    }

    private static TransactionTarget? CreateTarget(Money transactionMoneyAmount, NewTransactionRequest.Target? requestTarget, Domain.Account? account)
    {
        if (requestTarget is null) return null;

        if (account is null)
            return new TransactionTarget(transactionMoneyAmount, requestTarget.AccountNumber, requestTarget.BankCode);

        return new TransactionTarget(transactionMoneyAmount.TransferTo(account.Currency), account.Number, account.BankCode)
            { Account = account };
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

        return freeFunds >= transaction.WithdrawFrom.Money.Amount
            ? transaction
            : new DomainErrors.Account.NotEnoughFunds();
    }

    private Result<Transaction> AddFundsReservation(Transaction transaction)
    {
        if (transaction is not { WithdrawFrom.IsExternal: false }) return transaction;

        var fundsReservation = new FundsReservation
        {
            Amount = transaction.WithdrawFrom.Money.Amount,
            AccountNumber = transaction.WithdrawFrom.AccountNumber
        };
        context.Add(fundsReservation);
        return transaction.AddReservations(fundsReservation);
    }
}