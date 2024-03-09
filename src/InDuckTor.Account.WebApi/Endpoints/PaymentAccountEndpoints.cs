using InDuckTor.Account.WebApi.Models;
using InDuckTor.Shared.Security.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class PaymentAccountEndpoints
{
    public static IEndpointRouteBuilder AddPaymentAccountEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1")
            .WithTags("PaymentAccounts")
            .WithOpenApi()
            .RequireAuthorization();

        groupBuilder.MapGet("/account", GetCallingUserAccounts)
            .WithDescription("Получить все счёта текущего пользователя");

        groupBuilder.MapPost("/account", OpenNewAccount)
            .WithDescription("Открыть новый счёт для текущего пользователя");

        groupBuilder.MapGet("/account/{accountNumber}/transaction", GetAccountTransactions)
            .WithDescription("Получить трансакции по счёту для текущего пользователя");

        groupBuilder.MapPost("/account/transaction", MakeTransaction)
            .WithDescription("Совершить трансакцию по счёту текущего пользователя");

        groupBuilder.MapPut("/account/{accountNumber}/freeze", FreezeAccount)
            .WithDescription("Запрос пользователя заморозить счёт");

        groupBuilder.MapPut("/account/{accountNumber}/unfreeze", UnfreezeAccount)
            .WithDescription("Запрос пользователя разморозить счёт");

        groupBuilder.MapPost("/account/{accountNumber}/close", CloseAccount)
            .WithDescription("Запрос пользователя закрыть счёт");

        return builder;
    }


    internal static Ok<PaymentAccountDto[]> GetCallingUserAccounts(ISecurityContext securityContext)
    {
        throw new NotImplementedException();
    }

    internal static Results<Accepted, ForbidHttpResult> OpenNewAccount([FromBody] OpenPaymentAccountRequest request, ISecurityContext securityContext)
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

    internal static Results<Accepted, BadRequest, Conflict> MakeTransaction([FromBody] MakeTransactionRequest request)
    {
        throw new NotImplementedException();
    }

    internal static Results<NoContent, ForbidHttpResult> FreezeAccount([FromRoute] string accountNumber, ISecurityContext securityContext)
    {
        throw new NotImplementedException();
    }

    internal static Results<NoContent, ForbidHttpResult> UnfreezeAccount([FromRoute] string accountNumber, ISecurityContext securityContext)
    {
        throw new NotImplementedException();
    }

    internal static Results<NoContent, ForbidHttpResult> CloseAccount([FromRoute] string accountNumber, ISecurityContext securityContext)
    {
        throw new NotImplementedException();
    }
}