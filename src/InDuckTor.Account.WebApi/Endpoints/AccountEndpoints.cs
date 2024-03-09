using InDuckTor.Account.WebApi.Models;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class AccountEndpoints
{
    public static IEndpointRouteBuilder AddBankingAccountEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1")
            .WithTags("BankingAccounts")
            .WithOpenApi()
            .RequireAuthorization();

        groupBuilder.MapPut("/bank/account/search", SearchAccounts)
            .WithDescription("Поиск по всем счётам");

        groupBuilder.MapGet("/bank/account/{accountNumber}/transaction", GetAccountTransactions)
            .WithDescription("Получить трансакции по счёту");

        return builder;
    }

    internal static Results<Ok<CollectionSearchResult<AccountDto>>, ForbidHttpResult> SearchAccounts(
        [FromBody] AccountsSearchParams searchParams)
    {
        throw new NotImplementedException();
    }

    /// <remarks>Планируется переход на Keyset Pagination https://struchkov.dev/blog/ru/seek-method-or-keyset-pagination</remarks>
    internal static Results<Ok<TransactionDto[]>, ForbidHttpResult> GetAccountTransactions(
        [FromRoute] string accountNumber,
        [FromQuery] int? take,
        [FromQuery] int? skip)
    {
        throw new NotImplementedException();
    }
}