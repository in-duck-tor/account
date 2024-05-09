using System.Text;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Account.KafkaClient;
using InDuckTor.Shared.Idempotency.Http;
using InDuckTor.Shared.Kafka;
using Mapster;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.Extensions.ObjectPool;
using Microsoft.Extensions.Primitives;
using Microsoft.OpenApi.Models;
using CancelTransaction = InDuckTor.Account.Contracts.Public.CancelTransaction;
using CommitTransaction = InDuckTor.Account.Contracts.Public.CommitTransaction;
using CreateAccount = InDuckTor.Account.Contracts.Public.CreateAccount;
using OpenTransaction = InDuckTor.Account.Contracts.Public.OpenTransaction;

namespace InDuckTor.Account.WebApi.Endpoints;

public static partial class AccountEndpoints
{
    private static void AddV2(IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v2")
            .WithTags("BankingAccounts V2")
            .WithOpenApi()
            .RequireAuthorization()
            .WithIdempotencyKey(ttlSeconds: 8 * 60 * 60);

        groupBuilder.MapPost("/bank/account", CreateAccountV2)
            .WithName(nameof(CreateAccountV2))
            .WithDescription("Отправляет команду на создание счёта. Только для внешних клиентов");

        groupBuilder.MapPost("/bank/account/transaction", OpenTransactionV2)
            .WithName(nameof(OpenTransactionV2))
            .WithDescription("Отправляет команду на начало трансакции между счётами. Только для внешних клиентов");

        groupBuilder.MapPost("/bank/account/transaction/{transactionId}/commit", CommitTransactionV2)
            .WithName(nameof(CommitTransactionV2))
            .WithDescription("Отправляет команду на фисацию трансакции между счётами. Только для внешних клиентов");

        groupBuilder.MapPost("/bank/account/transaction/{transactionId}/cancel", CancelTransactionV2)
            .WithName(nameof(CancelTransactionV2))
            .WithDescription("Отправляет команду на отмену трансакции между счётами. Только для внешних клиентов");
    }

    internal static async Task<Accepted> CreateAccountV2(
        [FromBody] CreateAccountRequest request,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> commandProducer,
        CancellationToken ct)
    {
        await commandProducer.ProduceAccountCommand(new AccountCommandEnvelop { CreateAccount = request.Adapt<CreateAccount>() },
            cancellationToken: ct);
        return TypedResults.Accepted(null as string);
    }

    internal static async Task<Accepted> OpenTransactionV2(
        [FromBody] OpenTransactionRequest request,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> commandProducer,
        CancellationToken ct)
    {
        await commandProducer.ProduceAccountCommand(new AccountCommandEnvelop { OpenTransaction = request.Adapt<OpenTransaction>() },
            cancellationToken: ct);
        return TypedResults.Accepted(null as string);
    }

    internal static async Task<Accepted> CommitTransactionV2(
        [FromBody] CreateAccountRequest request,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> commandProducer,
        CancellationToken ct)
    {
        await commandProducer.ProduceAccountCommand(new AccountCommandEnvelop { CommitTransaction = request.Adapt<CommitTransaction>() },
            cancellationToken: ct);
        return TypedResults.Accepted(null as string);
    }

    internal static async Task<Accepted> CancelTransactionV2(
        [FromBody] CreateAccountRequest request,
        [FromServices] ITopicProducer<AccountCommandKey, AccountCommandEnvelop> commandProducer,
        CancellationToken ct)
    {
        await commandProducer.ProduceAccountCommand(new AccountCommandEnvelop { CancelTransaction = request.Adapt<CancelTransaction>() },
            cancellationToken: ct);
        return TypedResults.Accepted(null as string);
    }
}