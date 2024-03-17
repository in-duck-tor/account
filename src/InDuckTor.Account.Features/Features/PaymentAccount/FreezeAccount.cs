using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.PaymentAccount;

public readonly record struct FreezeAccountRequest(AccountNumber AccountNumber, bool Unfreeze = false);

/// <summary>
/// Принимает номер счёта и замораживает его
/// </summary>
public interface IFreezeAccount : ICommand<FreezeAccountRequest, Result>;

public class FreezeAccount(AccountsDbContext context, ISecurityContext securityContext) : IFreezeAccount
{
    public async Task<Result> Execute(FreezeAccountRequest request, CancellationToken ct)
    {
        var account = await context.Accounts.FindAsync([ request.AccountNumber ], ct);
        if (account is null) return new Errors.Account.NotFound(request.AccountNumber);

        if (!account.CanUserFreeze(securityContext.Currant)) return new Errors.Forbidden();

        var result = request.Unfreeze ? account.Unfreeze() : account.Freeze();
        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(ct);
        }

        return result;
    }
}