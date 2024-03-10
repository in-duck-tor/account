using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Account.GetAccountTransactions;

[Intercept(typeof(RequireReadAccountsPermission<GetAccountTransactionsParams, TransactionDto[]>))]
public class GetAccountTransactions(AccountsDbContext context) : IGetAccountTransactions
{
    // todo move to config
    private const int DefaultTake = 30;
    private const int MaxTake = 1000;

    public async Task<Result<TransactionDto[]>> Execute(GetAccountTransactionsParams input, CancellationToken ct)
    {
        // todo test nulls and implicit conversion
        var query = context.Transactions
            .Where(Specifications.Transaction.RelatedToAccount(input.AccountNumber))
            .Select(TransactionDto.Projection);

        query = input.Skip.HasValue ? query.Skip(input.Skip.Value) : query;
        query = input.Take.HasValue ? query.Take(int.Min(input.Take ?? DefaultTake, MaxTake)) : query;

        return await query.ToArrayAsync(ct);
    }
}