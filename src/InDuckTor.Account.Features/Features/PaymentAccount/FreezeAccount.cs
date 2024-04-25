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

public readonly record struct FreezeAccountRequest(AccountNumber AccountNumber, bool Unfreeze = false);

/// <summary>
/// Принимает номер счёта и замораживает его
/// </summary>
public interface IFreezeAccount : ICommand<FreezeAccountRequest, Result>;

public class FreezeAccount(AccountsDbContext context, ISecurityContext securityContext, ITopicProducer<string, AccountEnvelop> producer) : IFreezeAccount
{
    public async Task<Result> Execute(FreezeAccountRequest request, CancellationToken ct)
    {
        var account = await context.Accounts.FindAsync([ request.AccountNumber ], ct);
        if (account is null) return new DomainErrors.Account.NotFound(request.AccountNumber);

        if (!account.CanUserFreeze(securityContext.Currant)) return new Errors.Forbidden();

        var result = request.Unfreeze ? account.Unfreeze() : account.Freeze();
        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(ct);
            await producer.Produce(null!, account.ToStateChangedEventEnvelop(securityContext.Currant.Id), ct);
        }

        return result;
    }
}