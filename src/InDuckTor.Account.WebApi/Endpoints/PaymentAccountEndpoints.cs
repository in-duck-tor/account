namespace InDuckTor.Account.WebApi.Endpoints;

public static partial class PaymentAccountEndpoints
{
    public static IEndpointRouteBuilder AddPaymentAccountEndpoints(this IEndpointRouteBuilder builder)
    {
        AddV1(builder);
        AddV2(builder);

        return builder;
    }
}