using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Transactions;

/// <summary>
/// Принимает Id трансакции и коммитит её
/// </summary>
public interface ICommitTransaction : ICommand<long, Result>;

public class CommitTransaction(AccountsDbContext context) : ICommitTransaction
{
    public async Task<Result> Execute(long transactionId, CancellationToken ct)
    {
        var transaction = await context.Transactions.FindAsync([ transactionId ], ct);
        if (transaction is null) return new Errors.Transaction.NotFound(transactionId);

        if (transaction is { WithdrawFrom.InExternal: false })
        {
            var account = await context.Accounts.FindAsync([ transaction.WithdrawFrom.AccountNumber ], ct);
            account!.Amount -= transaction.WithdrawFrom.Amount;
        }

        if (transaction is { DepositOn.InExternal: false })
        {
            var account = await context.Accounts.FindAsync([ transaction.DepositOn.AccountNumber ], ct);
            account!.Amount += transaction.DepositOn.Amount;
        }

        // проверки прав нет, посчитаем Id трансакции уже секретом
        var result = transaction.Commit();
        if (!result.IsSuccess) return result.ToResult();
        
        var committedReservations = result.Value;
        context.FundsReservations.RemoveRange(committedReservations);
        await context.SaveChangesAsync(ct);
        return Result.Ok();
    }
}