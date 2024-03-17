using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Models;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using AccountType = InDuckTor.Shared.Security.Context.AccountType;

namespace InDuckTor.Account.Features.Account;

public class RequireAccountManagementPermission<TInput, TSuccess> : IStrategyInterceptor<TInput, Result<TSuccess>>
{
    private readonly ISecurityContext _securityContext;

    public RequireAccountManagementPermission(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    public Task<Result<TSuccess>> Intercept(TInput input, IStrategy<TInput, Result<TSuccess>>.Delegate next, CancellationToken ct)
    {
        if (_securityContext.Currant.AccountType is AccountType.System
            || true ) // todo introduce Permission.Account.Manage 
            return next(input, ct);
        return Task.FromResult<Result<TSuccess>>(new Errors.Forbidden());
    }
}

public class RequireReadAccountsPermission<TInput, TSuccess> : IStrategyInterceptor<TInput, Result<TSuccess>>
{
    private readonly ISecurityContext _securityContext;

    public RequireReadAccountsPermission(ISecurityContext securityContext)
    {
        _securityContext = securityContext;
    }

    public Task<Result<TSuccess>> Intercept(TInput input, IStrategy<TInput, Result<TSuccess>>.Delegate next, CancellationToken ct)
    {
        //todo
        return next(input, ct);
        return Task.FromResult<Result<TSuccess>>(new Errors.Forbidden());
    }
}