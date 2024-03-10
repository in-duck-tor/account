using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

public interface ICloseAccount : ICommand<string, Result>;

public class CloseAccount(AccountsDbContext context, ISecurityContext securityContext) : ICloseAccount
{
    public async Task<Result> Execute(string accountNumber, CancellationToken ct)
    {
        var account = await context.Accounts.FindAsync([ accountNumber ], ct);
        if (account is null) return new Errors.Account.NotFound(accountNumber);

        if (account.CanUserClose(securityContext.Currant)) return new Errors.Forbidden();
        if (account.Amount != 0) return new Errors.Account.NotEmpty(accountNumber);

        var result = account.Close();
        if (result.IsSuccess)
        {
            await context.SaveChangesAsync(ct);
        }

        return result;
    }
}