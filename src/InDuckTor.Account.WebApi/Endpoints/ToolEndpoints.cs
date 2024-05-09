using Hangfire;
using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Shared.Idempotency.Http;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Strategies;
using Microsoft.AspNetCore.Http.HttpResults;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class ToolEndpoints
{
    public static IEndpointRouteBuilder UseToolEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("api/tool")
            .WithTags("ToolEndpoints")
            .WithDescription("Инструментарий для ручного управления сервисом")
            .WithOpenApi();

        groupBuilder.MapPost("/pull-cbr-currencies", PullCbrCurrencies);
        groupBuilder.MapPost("/test-idempotency", TestIdempotency)
            .WithIdempotencyKey();

        return builder;
    }

    /// <summary>
    /// Ставит в очередь Hangfire задачу на обновление валют из ЦБР 
    /// </summary>
    /// <remarks>Не влияет на регулярную задачу обновления валют</remarks>
    /// <returns>Enqueued Hangfire job id</returns>
    internal static Accepted<string> PullCbrCurrencies(IBackgroundJobClient backgroundJobClient)
    {
        var jobId = backgroundJobClient.Enqueue((IExecutor<IPullCbrCurrencyRoutine, Unit, Unit> routine) => routine.Execute(new Unit(), default));
        return TypedResults.Accepted(null as string, jobId);
    }

    internal static Ok<DateTime> TestIdempotency()
    {
        return TypedResults.Ok(DateTime.UtcNow);
    }
}