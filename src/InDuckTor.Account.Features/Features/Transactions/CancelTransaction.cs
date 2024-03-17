using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Transactions;

public interface ICancelTransaction : ICommand<long, Result>;

public class CancelTransaction(AccountsDbContext context) : ICancelTransaction
{
    public async Task<Result> Execute(long transactionId, CancellationToken ct)
    {
        var transaction = await context.Transactions.FindAsync([ transactionId ], ct);
        if (transaction is null) return new Errors.Transaction.NotFound(transactionId);

        return await transaction.Cancel().Bind(DeleteReservations);

        async Task<Result> DeleteReservations(FundsReservation[] cancelledReservations)
        {
            context.FundsReservations.RemoveRange(cancelledReservations);
            await context.SaveChangesAsync(ct);
            return Result.Ok();
        }
    }
}