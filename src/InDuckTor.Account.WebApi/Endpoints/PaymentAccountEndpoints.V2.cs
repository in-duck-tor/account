using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.PaymentAccount;
using InDuckTor.Account.KafkaClient;
using InDuckTor.Account.WebApi.Mapping;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Mapster;
using Microsoft.AspNetCore.Mvc;
using FreezeAccount = InDuckTor.Account.Contracts.Public.FreezeAccount;

namespace InDuckTor.Account.WebApi.Endpoints;

public static partial class PaymentAccountEndpoints
{
    internal static void AddV2(IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v2")
            .WithTags("PaymentAccounts V2")
            .WithOpenApi()
            .RequireAuthorization();

        groupBuilder.MapPost("/account/transaction", MakeTransactionV2)
            .WithName(nameof(MakeTransactionV2))
            .WithDescription("Совершить трансакцию по счёту текущего пользователя");

        groupBuilder.MapPut("/account/{accountNumber}/freeze", FreezeAccountV2)
            .WithName(nameof(FreezeAccountV2))
            .WithDescription("Запрос пользователя заморозить счёт");

        groupBuilder.MapPut("/account/{accountNumber}/unfreeze", UnfreezeAccountV2)
            .WithName(nameof(UnfreezeAccountV2))
            .WithDescription("Запрос пользователя разморозить счёт");

        groupBuilder.MapPost("/account/{accountNumber}/close", CloseAccountV2)
            .WithName(nameof(CloseAccountV2))
            .WithDescription("Запрос пользователя закрыть счёт");
    }

    [ProducesResponseType(202)]
    internal static async Task<IResult> MakeTransactionV2(
        [FromBody] NewTransactionRequest request,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> producer,
        CancellationToken ct)
    {
        await producer.ProduceAccountCommand(new() { OpenTransaction = request.Adapt<OpenTransaction>() }, cancellationToken: ct);
        return TypedResults.StatusCode(202);
    }

    [ProducesResponseType(202)]
    internal static async Task<IResult> FreezeAccountV2(
        [FromRoute] AccountNumber accountNumber,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> producer,
        CancellationToken ct)
    {
        await producer.ProduceAccountCommand(new() { FreezeAccount = new() { AccountNumber = accountNumber, Unfreeze = false } },cancellationToken: ct);
        return TypedResults.StatusCode(202);
    }

    [ProducesResponseType(202)]
    internal static async Task<IResult> UnfreezeAccountV2(
        [FromRoute] string accountNumber,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> producer,
        CancellationToken ct)
    {
        await producer.ProduceAccountCommand(new() { FreezeAccount = new() { AccountNumber = accountNumber, Unfreeze = true } },cancellationToken: ct);         
        return TypedResults.StatusCode(202);
    }

    [ProducesResponseType(202)]
    internal static async Task<IResult> CloseAccountV2(
        [FromRoute] AccountNumber accountNumber,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> producer,
        CancellationToken ct)
    {
        await producer.ProduceAccountCommand(new() { CloseAccount = new() { AccountNumber = accountNumber } },cancellationToken: ct);
        return TypedResults.StatusCode(202);
    }
}