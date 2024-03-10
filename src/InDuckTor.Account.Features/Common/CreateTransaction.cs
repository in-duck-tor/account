using FluentResults;
using FluentResults.Extensions;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Common;

public interface ICreateTransaction : ICommand<NewTransactionRequest, Result<Transaction>>;

// todo add ForUpdates
public class CreateTransaction(AccountsDbContext context, ISecurityContext securityContext) : ICreateTransaction
{
    public async Task<Result<Transaction>> Execute(NewTransactionRequest input, CancellationToken ct)
    {
        return await CreateNew(input, ct)
            .Bind(transaction => EnsureAccountAmount(transaction, ct))
            .Bind(AddFundsReservation);
    }

    private async ValueTask<Result<Transaction>> CreateNew(NewTransactionRequest input, CancellationToken ct)
    {
        if (input is not { WithdrawFrom.BankCode: Domain.BankInfo.InDuckTorBankCode }
            && input is not { DepositOn.BankCode: Domain.BankInfo.InDuckTorBankCode })
            return new Errors.InvalidInput("Необходимо указать известный счёт отправитель или счёт получатель");

        TransactionTarget? withdrawFrom = null;
        if (input.WithdrawFrom is { BankCode: Domain.BankInfo.InDuckTorBankCode })
        {
            var withdrawAccountNumber = input.WithdrawFrom.AccountNumber;

            var account = await context.Accounts.FindAsync([ withdrawAccountNumber ], ct);
            if (account is null) return new Errors.Account.NotFound(withdrawAccountNumber);

            if (!account.CanUserWithdraw(securityContext.Currant)) return new Errors.Forbidden();

            withdrawFrom = new TransactionTarget(input.Amount, account.Number, account.CurrencyCode, account.BankCode);
        }

        TransactionTarget? depositOn = null;
        if (input.DepositOn is { BankCode: Domain.BankInfo.InDuckTorBankCode })
        {
            var depositAccountNumber = input.DepositOn.AccountNumber;

            var account = await context.Accounts.FindAsync([ depositAccountNumber ], ct);
            if (account is null) return new Errors.Account.NotFound(depositAccountNumber);

            depositOn = new TransactionTarget(input.Amount, account.Number, account.CurrencyCode, account.BankCode);
        }

        withdrawFrom ??= new TransactionTarget(input.Amount, input.WithdrawFrom!.AccountNumber, depositOn!.CurrencyCode, input.WithdrawFrom.BankCode);
        depositOn ??= new TransactionTarget(input.Amount, input.DepositOn!.AccountNumber, withdrawFrom!.CurrencyCode, input.DepositOn.BankCode);

        if (withdrawFrom.CurrencyCode != depositOn.CurrencyCode) throw new NotImplementedException("Переводы с разными валютами не реализованы");

        return new Transaction(depositOn, withdrawFrom);
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