using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;
using AccountType = InDuckTor.Account.Domain.AccountType;

namespace InDuckTor.Account.Features.PaymentAccount;

public record GetAccountTransactionsParams(AccountNumber AccountNumber, int? Take, int? Skip);

public interface IGetAccountTransactions : IQuery<GetAccountTransactionsParams, Result<TransactionDto[]>>;

public class GetAccountTransactions(AccountsDbContext context, ISecurityContext securityContext) : IGetAccountTransactions
{
    // todo move to config
    private const int DefaultTake = 30;
    private const int MaxTake = 1000;

    public async Task<Result<TransactionDto[]>> Execute(GetAccountTransactionsParams input, CancellationToken ct)
    {
        var account = await context.Accounts.FirstOrDefaultAsync(x => x.Number == input.AccountNumber && x.Type == AccountType.Payment, ct);
        
        if (account is null) return new DomainErrors.Account.NotFound(input.AccountNumber);

        if (!account.CanUserRead(securityContext.Currant)) return new Errors.Forbidden();

        var query = context.Transactions
            .Where(Specifications.Transaction.RelatedToAccount(input.AccountNumber))
            .Select(TransactionDto.Projection);

        query = input.Skip.HasValue ? query.Skip(input.Skip.Value) : query;
        query = input.Take.HasValue ? query.Take(int.Min(input.Take ?? DefaultTake, MaxTake)) : query;

        return await query.ToArrayAsync(ct);
    }
}