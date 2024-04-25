using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Interceptors;
using Microsoft.EntityFrameworkCore;

namespace InDuckTor.Account.Features.Account.SearchAccounts;

[AllowSystem]
[RequirePermission(Permission.Account.Read)]
public class SearchAccounts(AccountsDbContext context) : ISearchAccounts
{
    // todo move to config
    private const int DefaultTake = 30;
    private const int MaxTake = 1000;

    public async Task<Result<CollectionSearchResult<AccountDto>>> Execute(AccountsSearchParams input, CancellationToken ct)
    {
        var query = context.Accounts.AsQueryable();

        query = input.OwnerId.HasValue ? query.Where(x => x.OwnerId == input.OwnerId.Value) : query;
        query = input.AccountType.HasValue ? query.Where(x => x.Type == input.AccountType.Value) : query;
        query = input.AccountState.HasValue ? query.Where(x => x.State == input.AccountState.Value) : query;
        var countQuery = query;

        query = input.Skip.HasValue ? query.Skip(input.Skip.Value) : query;
        query = input.Take.HasValue ? query.Take(int.Min(input.Take ?? DefaultTake, MaxTake)) : query;
        var entities = await query.ToListAsync(ct);
        var items = entities.Select(x => new AccountDto(
                x.Number,
                x.CurrencyCode,
                x.BankCode,
                x.OwnerId,
                x.CreatedBy,
                x.Amount,
                x.State,
                x.Type,
                x.CustomComment,
                x.GrantedUsers.Select(user => new AccountDto.GrantedUser(user.Id, user.Actions)).ToArray()))
            .ToList();

        return new CollectionSearchResult<AccountDto>(await countQuery.CountAsync(ct), items);
    }
}