using FluentResults;

namespace InDuckTor.Account.Features.Utils;

public static class ResultExtensions
{
    public static Result<(T1 first, T2 second)> Merge<T1, T2>(this Result<T1> first, Result<T2> second)
    {
        Result<(T1 first, T2 second)> result = new();
        if (first.IsFailed) result = result.WithReasons(first.Reasons);
        if (second.IsFailed) result = result.WithReasons(second.Reasons);
        return result.IsFailed
            ? result
            : result.WithValue((first.Value, second.Value));
    }
}