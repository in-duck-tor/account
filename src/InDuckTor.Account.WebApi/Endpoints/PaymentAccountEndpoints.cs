﻿using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.PaymentAccount;
using InDuckTor.Account.WebApi.Mapping;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using IResult = Microsoft.AspNetCore.Http.IResult;

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


    internal static async Task<Ok<PaymentAccountDto[]>> GetCallingUserAccounts(
        [FromServices] IExecutor<IGetCallingUserAccounts, Unit, PaymentAccountDto[]> getCallingUserAccounts,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await getCallingUserAccounts.Execute(new Unit(), cancellationToken));
    }

    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(202)]
    internal static async Task<IResult> OpenNewAccount(
        [FromBody] OpenPaymentAccountRequest request,
        [FromServices] IExecutor<IOpenNewAccount, OpenPaymentAccountRequest, Result<CreateAccountResult>> openPaymentAccount,
        CancellationToken cancellationToken)
    {
        var result = await openPaymentAccount.Execute(request, cancellationToken);
        return result.MapToHttpResult(TypedResults.Ok);
    }

    /// <remarks>Планируется переход на Keyset Pagination https://struchkov.dev/blog/ru/seek-method-or-keyset-pagination</remarks>
    /// <response code="404">Счёт не найден</response>
    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(202)]
    [Produces<TransactionDto[]>]
    internal static async Task<IResult> GetAccountTransactions(
        [FromRoute] string accountNumber,
        [FromQuery] int? take,
        [FromQuery] int? skip,
        [FromServices] IExecutor<IGetAccountTransactions, GetAccountTransactionsParams, Result<TransactionDto[]>> getAccountTransactions,
        CancellationToken cancellationToken)
    {
        var result = await getAccountTransactions.Execute(new(accountNumber, take, skip), cancellationToken);
        return result.MapToHttpResult(TypedResults.Ok);
    }

    [ProducesResponseType(400)]
    [ProducesResponseType(404)]
    [ProducesResponseType(409)]
    [ProducesResponseType<MakeTransactionResult>(202)]
    internal static async Task<IResult> MakeTransaction(
        [FromBody] NewTransactionRequest request,
        [FromServices] IExecutor<IMakeTransaction, NewTransactionRequest, Result<MakeTransactionResult>> makeTransaction,
        CancellationToken cancellationToken)
    {
        var result = await makeTransaction.Execute(request, cancellationToken);
        return result.MapToHttpResult(idResult => TypedResults.Accepted(null as string, idResult));
    }

    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(204)]
    internal static async Task<IResult> FreezeAccount(
        [FromRoute] string accountNumber,
        [FromServices] IExecutor<IFreezeAccount, FreezeAccountRequest, Result> freeze,
        CancellationToken cancellationToken)
    {
        var result = await freeze.Execute(new FreezeAccountRequest(accountNumber), cancellationToken);
        return result.MapToHttpResult(TypedResults.NoContent);
    }

    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(204)]
    internal static async Task<IResult> UnfreezeAccount(
        [FromRoute] string accountNumber,
        [FromServices] IExecutor<IFreezeAccount, FreezeAccountRequest, Result> freeze,
        CancellationToken cancellationToken)
    {
        var result = await freeze.Execute(new FreezeAccountRequest(accountNumber, true), cancellationToken);
        return result.MapToHttpResult(TypedResults.NoContent);
    }

    [ProducesResponseType(404)]
    [ProducesResponseType(403)]
    [ProducesResponseType(204)]
    internal static async Task<IResult> CloseAccount(
        [FromRoute] AccountNumber accountNumber,
        [FromServices] IExecutor<ICloseAccount, AccountNumber, Result> close,
        CancellationToken cancellationToken)
    {
        var result = await close.Execute(accountNumber, cancellationToken);
        return result.MapToHttpResult(TypedResults.NoContent);
    }
} 