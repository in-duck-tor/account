﻿using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Transactions;

public interface ICancelTransaction : ICommand<long, Result>;

public class CancelTransaction(
    AccountsDbContext context,
    ITransactionEventsProducer producer) : ICancelTransaction
{
    public async Task<Result> Execute(long transactionId, CancellationToken ct)
    {
        var transaction = await context.Transactions.Include(x => x.Reservations)
            .FirstOrDefaultAsync(x => x.Id == transactionId, ct);
        if (transaction is null) return new DomainErrors.Transaction.NotFound(transactionId);

        return await transaction.Cancel().Bind(DeleteReservations);

        async Task<Result> DeleteReservations(FundsReservation[] cancelledReservations)
        {
            context.FundsReservations.RemoveRange(cancelledReservations);
            await context.SaveChangesAsync(ct);
            
            await producer.ProduceTransactionFinished(transaction, ct);
            
            return Result.Ok();
        }
    }
}