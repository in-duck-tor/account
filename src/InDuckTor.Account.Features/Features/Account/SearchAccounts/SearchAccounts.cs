using FluentResults;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Strategies;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Account.SearchAccounts;

[Intercept(typeof(RequireReadAccountsPermission<AccountsSearchParams, CollectionSearchResult<AccountDto>>))]
public class SearchAccounts(AccountsDbContext context) : ISearchAccounts
{
    // todo move to config
    private const int DefaultTake = 30;
    private const int MaxTake = 1000;

    public async Task<Result<CollectionSearchResult<AccountDto>>> Execute(AccountsSearchParams input, CancellationToken ct)
    {
        var query = context.Accounts.AsQueryable();

        query = input.OwnerId.HasValue ? query.Where(x => x.OwnerId == input.OwnerId) : query;
        query = input.AccountType.HasValue ? query.Where(x => x.Type == input.AccountType) : query;
        query = input.AccountState.HasValue ? query.Where(x => x.State == input.AccountState) : query;
        var countQuery = query;

        query = input.Skip.HasValue ? query.Skip(input.Skip.Value) : query;
        query = input.Take.HasValue ? query.Take(int.Min(input.Take ?? DefaultTake, MaxTake)) : query;
        var itemsQuery = query.Select(x => new AccountDto(
            x.Number,
            x.Currency.Code,
            x.BankCode,
            x.OwnerId,
            x.CreatedBy,
            x.Amount,
            x.State,
            x.Type,
            x.CustomComment,
            x.GrantedUsers.Select(user => new AccountDto.GrantedUser(user.Id, user.Actions)).ToArray()));

        return new CollectionSearchResult<AccountDto>(
            Total: await countQuery.CountAsync(ct),
            Items: await itemsQuery.ToListAsync(ct));
    }
}