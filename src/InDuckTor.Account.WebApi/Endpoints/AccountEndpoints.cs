using System.ComponentModel;
using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.Account.GetAccountTransactions;
using InDuckTor.Account.Features.Account.SearchAccounts;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Account.WebApi.Mapping;
using InDuckTor.Shared.Strategies;
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

        groupBuilder.MapPost("/bank/account", CreateAccount)
            .WithDescription("Создать счёт");

        groupBuilder.MapPut("/bank/account/search", SearchAccounts)
            .WithDescription("Поиск по всем счётам");

        groupBuilder.MapGet("/bank/account/{accountNumber}/transaction", GetAccountTransactions)
            .WithDescription("Получить трансакции по счёту");

        groupBuilder.MapPost("/bank/account/transaction", OpenTransaction)
            .WithDescription("Начать трансакцию между счётами");

        groupBuilder.MapPost("/bank/account/transaction/{transactionId}/commit", CommitTransaction)
            .WithDescription("Зафиксировать трансакцию между счётами");

        groupBuilder.MapPost("/bank/account/transaction/{transactionId}/cancel", CancelTransaction)
            .WithDescription("Отменить трансакцию между счётами");

        return builder;
    }

    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType<CreateAccountResult>(200)]
    internal static async Task<IResult> CreateAccount(
        [FromBody] CreateAccountRequest request,
        [FromServices] IExecutor<ICreateAccount, CreateAccountRequest, Result<CreateAccountResult>> createAccount,
        CancellationToken cancellationToken)
    {
        var result = await createAccount.Execute(request, cancellationToken);
        return result.MapToHttpResult(TypedResults.Ok);
    }

    [ProducesResponseType(403)]
    [ProducesResponseType<CollectionSearchResult<AccountDto>>(200)]
    internal static async Task<IResult> SearchAccounts(
        [FromBody] AccountsSearchParams searchParams,
        [FromServices] IExecutor<ISearchAccounts, AccountsSearchParams, Result<CollectionSearchResult<AccountDto>>> searchAccounts,
        CancellationToken cancellationToken)
    {
        var result = await searchAccounts.Execute(searchParams, cancellationToken);
        return result.MapToHttpResult(TypedResults.Ok);
    }

    /// <remarks>Планируется переход на Keyset Pagination https://struchkov.dev/blog/ru/seek-method-or-keyset-pagination</remarks>
    [ProducesResponseType(403)]
    [ProducesResponseType<TransactionDto[]>(200)]
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

    [ProducesResponseType<OpenTransactionResult>(202)]
    internal static async Task<IResult> OpenTransaction(
        [FromBody] OpenTransactionRequest request,
        [FromServices] IExecutor<IOpenTransaction, OpenTransactionRequest, Result<OpenTransactionResult>> openTransaction,
        CancellationToken cancellationToken)
    {
        var result = await openTransaction.Execute(request, cancellationToken);
        return result.MapToHttpResult(transactionResult => TypedResults.Accepted(null as string, transactionResult));
    }


    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(204)]
    internal static async Task<IResult> CommitTransaction(
        long transactionId,
        [FromServices] IExecutor<ICommitTransaction, long, Result> commitTransaction,
        CancellationToken cancellationToken)
    {
        var result = await commitTransaction.Execute(transactionId, cancellationToken);
        return result.MapToHttpResult(TypedResults.NoContent);
    }

    [ProducesResponseType(403)]
    [ProducesResponseType(404)]
    [ProducesResponseType(204)]
    internal static async Task<IResult> CancelTransaction(
        long transactionId,
        [FromServices] IExecutor<ICancelTransaction, long, Result> cancelTransaction,
        CancellationToken cancellationToken)
    {
        var result = await cancelTransaction.Execute(transactionId, cancellationToken);
        return result.MapToHttpResult(TypedResults.NoContent);
    }
}