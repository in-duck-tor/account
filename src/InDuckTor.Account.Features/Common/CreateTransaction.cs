using FluentResults;
using FluentResults.Extensions;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
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
                return transaction;
            });

    // todo refactor
    private async ValueTask<Result<Transaction>> CreateNew(NewTransactionRequest request, TimeSpan ttl, CancellationToken ct)
    {
        if (request is not { WithdrawFrom.BankCode: Domain.BankInfo.InDuckTorBankCode }
            && request is not { DepositOn.BankCode: Domain.BankInfo.InDuckTorBankCode })
            return new Errors.InvalidInput("Необходимо указать известный счёт отправитель или счёт получатель");

        var currantUser = securityContext.Currant;
        TransactionTarget? withdrawFrom = null;
        if (request.WithdrawFrom is { BankCode: Domain.BankInfo.InDuckTorBankCode })
        {
            var withdrawAccountNumber = request.WithdrawFrom.AccountNumber;

            var account = await context.Accounts.FindAsync([ withdrawAccountNumber ], ct);
            if (account is null) return new Errors.Account.NotFound(withdrawAccountNumber);

            if (!account.CanUserWithdraw(currantUser)) return new Errors.Forbidden();

            withdrawFrom = new TransactionTarget(request.Amount, account.Number, account.CurrencyCode, account.BankCode);
        }

        TransactionTarget? depositOn = null;
        if (request.DepositOn is { BankCode: Domain.BankInfo.InDuckTorBankCode })
        {
            var depositAccountNumber = request.DepositOn.AccountNumber;

            var account = await context.Accounts.FindAsync([ depositAccountNumber ], ct);
            if (account is null) return new Errors.Account.NotFound(depositAccountNumber);

            if (!account.CanUserDeposit(currantUser)) return new Errors.Forbidden();

            depositOn = new TransactionTarget(request.Amount, account.Number, account.CurrencyCode, account.BankCode);
        }

        withdrawFrom ??= new TransactionTarget(request.Amount, request.WithdrawFrom!.AccountNumber, depositOn!.CurrencyCode, request.WithdrawFrom.BankCode);
        depositOn ??= new TransactionTarget(request.Amount, request.DepositOn!.AccountNumber, withdrawFrom!.CurrencyCode, request.DepositOn.BankCode);

        if (withdrawFrom.CurrencyCode != depositOn.CurrencyCode) throw new NotImplementedException("Переводы с разными валютами не реализованы");

        return new Transaction(depositOn, withdrawFrom, ttl, currantUser.Id);
    }

    private async ValueTask<Result<Transaction>> EnsureAccountAmount(Transaction transaction, CancellationToken ct)
    {
        if (transaction is not { WithdrawFrom.InExternal: false }) return transaction;

        var account = await context.Accounts.FindAsync([ transaction.WithdrawFrom.AccountNumber ], ct);
        var fundsReservations = await context.FundsReservations
            .Where(x => x.AccountNumber == account!.Number)
            .ToListAsync(ct);

        var reserved = fundsReservations.Aggregate(new decimal(), (sum, reservation) => sum + reservation.Amount);
        var freeFunds = account!.Amount - reserved;

        return freeFunds >= transaction.WithdrawFrom.Amount
            ? transaction
            : new Errors.Account.NotEnoughFunds();
    }

    private Result<Transaction> AddFundsReservation(Transaction transaction)
    {
        if (transaction is not { WithdrawFrom.InExternal: false }) return transaction;

        var fundsReservation = new FundsReservation
        {
            Amount = transaction.WithdrawFrom.Amount,
            AccountNumber = transaction.WithdrawFrom.AccountNumber
        };
        context.Add(fundsReservation);
        return transaction.AddReservations(fundsReservation);
    }
}