using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Interceptors;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Models;
using InDuckTor.Shared.Protobuf;
using InDuckTor.Shared.Security.Context;

namespace InDuckTor.Account.KafkaClient;

public class AccountCommandSecurityContextEnrich : ITopicProducerInterceptor<AccountCommandKey, AccountCommandEnvelop>
{
    private readonly ISecurityContext _securityContext;

    public AccountCommandSecurityContextEnrich(ISecurityContext securityContext) => _securityContext = securityContext;

    public Task<Unit> Intercept(Message<AccountCommandKey, AccountCommandEnvelop> message, IInterceptor<Message<AccountCommandKey, AccountCommandEnvelop>, Unit>.Delegate next, CancellationToken ct)
    {
        if (message.Value.CallingUser == null && _securityContext.IsImpersonated)
        {
            message.Value.CallingUser = UserPrincipal.FromClaims(_securityContext.Currant.Claims);
        }

        return next(message, ct);
    }
}