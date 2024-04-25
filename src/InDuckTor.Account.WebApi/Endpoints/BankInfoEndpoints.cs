using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Account.Telemetry;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class BankInfoEndpoints
{
    public static IEndpointRouteBuilder AddBankInfoEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1")
            .WithTags("BankInfo")
            .WithOpenApi();

        groupBuilder.MapGet("/bank", GetBanksInfo)
            .WithName(nameof(GetBanksInfo))
            .WithDescription("Получение информации об известных банках");

        groupBuilder.MapGet("/bank/currency", GetCurrenciesInfo)
            .WithName(nameof(GetCurrenciesInfo))
            .WithDescription("Получение информации об известных валютах");

        return builder;
    }

    internal static async Task<Ok<BankInfo[]>> GetBanksInfo(
        [FromServices] IExecutor<IGetBanksInfo, Unit, BankInfo[]> getBanksInfo,
        CancellationToken cancellationToken)
    {
        using var _ = TelemetryGlobals.ActivitySource.StartActivity();
        return TypedResults.Ok(await getBanksInfo.Execute(new Unit(), cancellationToken));
    }

    internal static async Task<Ok<CurrencyInfo[]>> GetCurrenciesInfo(
        [FromServices] IExecutor<IGetCurrenciesInfo, Unit, CurrencyInfo[]> getCurrenciesInfo,
        CancellationToken cancellationToken)
    {
        return TypedResults.Ok(await getCurrenciesInfo.Execute(new Unit(), cancellationToken));
    }
}