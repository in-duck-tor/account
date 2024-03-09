using Microsoft.AspNetCore.Http.HttpResults;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class BankInfoEndpoints
{
    public static IEndpointRouteBuilder AddBankInfoEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("/api/v1")
            .WithTags("BankInfo")
            .WithOpenApi();

        groupBuilder.MapGet("/bank", GetBanksInfo)
            .WithDescription("Получение информации об известных банках");

        groupBuilder.MapGet("/bank/currency", GetCurrenciesInfo)
            .WithDescription("Получение информации об известных валютах");

        return builder;
    }

    internal static Ok<BankInfo[]> GetBanksInfo()
    {
        throw new NotImplementedException();
    }

    internal static Ok<CurrencyInfo[]> GetCurrenciesInfo()
    {
        throw new NotImplementedException();
    }
}