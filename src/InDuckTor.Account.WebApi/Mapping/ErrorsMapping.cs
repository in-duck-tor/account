using FluentResults;
using InDuckTor.Shared.Models;

namespace InDuckTor.Account.WebApi.Mapping;

// todd move to shared 
public static class ErrorsMapping
{
    public static IResult MapToHttpResult<TSuccess, T>(this Result<T> result, Func<T, TSuccess> onSuccess)
        where TSuccess : IResult
        => result.IsSuccess ? onSuccess(result.Value) : result.MapToErrorHttpResult();

    public static IResult MapToHttpResult<TSuccess>(this Result result, Func<TSuccess> onSuccess)
        where TSuccess : IResult
        => result.IsSuccess ? onSuccess() : result.MapToErrorHttpResult();

    public static IResult MapToErrorHttpResult(this IResultBase result)
    {
        var error = result.Errors.FirstOrDefault();
        return error switch
        {
            Errors.Forbidden => TypedResults.Problem(statusCode: 403, title: error.Message),
            Errors.NotFound => TypedResults.NotFound(error.Message),
            Errors.Conflict => TypedResults.Conflict(error.Message),
            Errors.InvalidInput invalidInput => TypedResults.ValidationProblem(title: error.Message, errors: invalidInput.ProduceFieldsErrors()),
            _ => TypedResults.Problem(result.ToString())
        };
    }
}