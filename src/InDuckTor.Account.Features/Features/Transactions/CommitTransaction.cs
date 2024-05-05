using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Transactions;

/// <summary>
/// Принимает Id трансакции и коммитит её
/// </summary>
public interface ICommitTransaction : ICommand<long, Result>;

public class CommitTransaction(
    AccountsDbContext context,
    ITransactionEventsProducer producer) : ICommitTransaction
{
    public async Task<Result> Execute(long transactionId, CancellationToken ct)
    {
        var transaction = await context.Transactions
            .Include(x => x.Reservations)
            // .Include(x => x.DepositOn!.Account)
            // .Include(x => x.WithdrawFrom!.Account)
            .FirstOrDefaultAsync(x => x.Id == transactionId, ct);
        if (transaction is null) return new DomainErrors.Transaction.NotFound(transactionId);

        if (transaction is { WithdrawFrom.IsExternal: false })
        {
            var account = await context.Accounts.FindAsync([ transaction.WithdrawFrom.AccountNumber ], ct);
            account!.Money -= transaction.WithdrawFrom.Money;
        }

        if (transaction is { DepositOn.IsExternal: false })
        {
            var account = await context.Accounts.FindAsync([ transaction.DepositOn.AccountNumber ], ct);
            account!.Money += transaction.DepositOn.Money;
        }

        // проверки прав нет, посчитаем Id трансакции уже секретом
        var result = transaction.Commit();
        if (!result.IsSuccess) return result.ToResult();
        
        var committedReservations = result.Value;
        context.FundsReservations.RemoveRange(committedReservations);
        await context.SaveChangesAsync(ct);
        
        await producer.ProduceTransactionFinished(transaction, ct);
        
        return Result.Ok();
    }
}