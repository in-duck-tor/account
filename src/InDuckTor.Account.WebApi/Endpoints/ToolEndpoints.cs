using FluentResults;
using Google.Protobuf.WellKnownTypes;
using Hangfire;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.BankInfo;
using InDuckTor.Account.Features.Models;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Protobuf;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Microsoft.AspNetCore.Http.HttpResults;
using AccountType = InDuckTor.Account.Domain.AccountType;
using CreateAccount = InDuckTor.Account.Contracts.Public.CreateAccount;

namespace InDuckTor.Account.WebApi.Endpoints;

public static class ToolEndpoints
{
    public static IEndpointRouteBuilder AddToolEndpoints(this IEndpointRouteBuilder builder)
    {
        var groupBuilder = builder.MapGroup("api/tool")
            .WithTags("ToolEndpoints")
            .WithDescription("Инструментарий для ручного управления сервисом")
            .WithOpenApi();

        groupBuilder.MapPost("/pull-cbr-currencies", PullCbrCurrencies);

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
}