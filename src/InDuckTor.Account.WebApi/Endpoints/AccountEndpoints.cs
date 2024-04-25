namespace InDuckTor.Account.WebApi.Endpoints;
public static partial class AccountEndpoints
{
    public static IEndpointRouteBuilder AddBankingAccountEndpoints(this IEndpointRouteBuilder builder)
    {
        AddV1(builder);
        AddV2(builder);

        return builder;
    }
}