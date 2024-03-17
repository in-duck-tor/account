using FluentResults;

namespace InDuckTor.Account.Features.Utils;

public static class ResultExtensions
{
    public static Result<(T first, T second)> Merge<T>(this Result<T> first, Result<T> second)
    {
        Result<(T first, T second)> result = new();
        if (first.IsFailed) result = result.WithReasons(first.Reasons);
        if (second.IsFailed) result = result.WithReasons(second.Reasons);
        return result.IsFailed
            ? result
            : result.WithValue((first.Value, second.Value));
    }
}