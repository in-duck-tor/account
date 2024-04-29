using Confluent.Kafka;
using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.PaymentAccount;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Shared.Interceptors;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Kafka.Interceptors;
using InDuckTor.Shared.Protobuf;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies;
using Mapster;

namespace InDuckTor.Account.Worker.Consumers;

[RetryStrategyStatic(10, [ 1 ])]
[Intercept(typeof(ConversationConsumerInterceptor<string, AccountCommandEnvelop>))]
public class AccountCommandsConsumer : IConsumerStrategy<string, AccountCommandEnvelop>
{
    private readonly ISecurityContext _securityContext;
    private readonly IServiceProvider _serviceProvider;
    private readonly ITopicProducer<string, CommandHandlingProblemDetails> _onFailProducer;
    private readonly ILogger<AccountCommandsConsumer> _logger;

    public AccountCommandsConsumer(ISecurityContext securityContext, IServiceProvider serviceProvider, ITopicProducer<string, CommandHandlingProblemDetails> onFailProducer, ILogger<AccountCommandsConsumer> logger)
    {
        _securityContext = securityContext;
        _serviceProvider = serviceProvider;
        _onFailProducer = onFailProducer;
        _logger = logger;
    }

    public async Task<ProcessingResult> Execute(ConsumeResult<string, AccountCommandEnvelop> input, CancellationToken ct)
    {
        var userClaims = input.Message.Value.CallingUser.ExtractClaims();
        using var impersonateScope = UserContext.TryCreateFromClaims(userClaims, out var userContext)
            ? _securityContext.Impersonate(userContext)
            : null;

        ResultBase result;
        switch (input.Message.Value.PayloadCase)
        {
            case AccountCommandEnvelop.PayloadOneofCase.CreateAccount:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<ICreateAccount, CreateAccountRequest, Result<CreateAccountResult>>>()
                    .Execute(input.Message.Value.CreateAccount.Adapt<CreateAccountRequest>(), ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.CloseAccount:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<ICloseAccount, AccountNumber, Result>>()
                    .Execute(input.Message.Value.CloseAccount.AccountNumber, ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.FreezeAccount:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<IFreezeAccount, FreezeAccountRequest, Result>>()
                    .Execute(input.Message.Value.FreezeAccount.Adapt<FreezeAccountRequest>(), ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.OpenTransaction:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<IOpenTransaction, OpenTransactionRequest, Result<OpenTransactionResult>>>()
                    .Execute(input.Message.Value.OpenTransaction.Adapt<OpenTransactionRequest>(), ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.CommitTransaction:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<ICommitTransaction, long, Result>>()
                    .Execute(input.Message.Value.CommitTransaction.TransactionId, ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.CancelTransaction:
                result = await _serviceProvider
                    .GetRequiredService<IExecutor<ICancelTransaction, long, Result>>()
                    .Execute(input.Message.Value.CancelTransaction.TransactionId, ct);
                break;
            case AccountCommandEnvelop.PayloadOneofCase.None:
            default:
                _logger.LogInformation("Команда неизвестного типа {AccountCommandType} пропушена", input.Message.Value.PayloadCase);
                result = Result.Ok();
                break;
        }
        
        if (result.IsFailed)
        {
            var fail = new CommandHandlingProblemDetails
            {
                Type = $"{input.Message.Value.PayloadCase}/unknown", // todo добавить коды\типы доменных ошибок
                Title = string.Join('\n', result.Errors.Select(error => error.Message)),
                Reasons = { result.Reasons.Select(reason => reason.Message) }
            };
            await _onFailProducer.Produce(input.Message.Key, fail, ct);

            // domain error - no retry
            return ProcessingResult.Skip;
        }

        return ProcessingResult.Ok;
    }
}