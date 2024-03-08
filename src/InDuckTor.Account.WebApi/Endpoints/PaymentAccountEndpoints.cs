using InDuckTor.Account.WebApi.Contracts;
using InDuckTor.Shared.Security.Context;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class PaymentAccountEndpoints
{
    public static IEndpointRouteBuilder AddPaymentAccountEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1")
            .WithTags("Accounts")
            .WithOpenApi();

        groupBuilder.MapGet("/account", GetCallingUserAccounts)
            .WithDescription("Получить все счёта текущего пользователя");

        groupBuilder.MapGet(".account{accountNumber}/transaction", GetAccountTransactions)
            .WithDescription("Получить трансакция по счёту");

        groupBuilder.MapPost("/account/transaction", MakeTransaction)
            .WithDescription("Совершить трансакцияю по счёту текущего пользователя");

        return builder;
    }


    internal static Ok<AccountDto> GetCallingUserAccounts(ISecurityContext securityContext)
    {
        throw new NotImplementedException();
    }

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
}