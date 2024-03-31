using Confluent.Kafka;
using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.PaymentAccount;

public interface ICloseAccount : ICommand<AccountNumber, Result>;

public class CloseAccount(AccountsDbContext context, ISecurityContext securityContext, ITopicProducer<Null, AccountEnvelop> producer) : ICloseAccount
{
    public async Task<Result> Execute(AccountNumber accountNumber, CancellationToken ct)
    {
        var account = await context.Accounts.FindAsync([ accountNumber ], ct);
        if (account is null) return new DomainErrors.Account.NotFound(accountNumber);

        if (!account.CanUserClose(securityContext.Currant)) return new Errors.Forbidden();
        if (account.Amount != 0) return new DomainErrors.Account.NotEmpty(accountNumber);

        var result = account.Close();
        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(ct);
            await producer.Produce(null!, account.ToStateChangedEventEnvelop(securityContext.Currant.Id), ct);
        }

        return result;
    }
}