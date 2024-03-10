using FluentResults;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Interceptors;

// todo create shared generic permission interceptor
public class RequirePermission<TInput, TSuccess> : IStrategyInterceptor<TInput, Result<TSuccess>>
{
    private readonly ISecurityContext _securityContext;

    public RequirePermission(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    public Task<Result<TSuccess>> Intercept(TInput input, IStrategy<TInput, Result<TSuccess>>.Delegate next, CancellationToken ct)
    {
        throw new NotImplementedException();
    }
}