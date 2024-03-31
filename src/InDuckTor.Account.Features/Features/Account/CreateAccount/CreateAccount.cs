using Confluent.Kafka;
using FluentResults;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Common;
using InDuckTor.Account.Features.Mapping;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Infrastructure.Database;
using InDuckTor.Shared.Kafka;
using InDuckTor.Shared.Security.Context;
using InDuckTor.Shared.Strategies.Interceptors;
using AccountAction = InDuckTor.Account.Domain.AccountAction;
using GrantedAccountUser = InDuckTor.Account.Domain.GrantedAccountUser;

namespace InDuckTor.Account.Features.Account.CreateAccount;

[AllowSystem]
[RequirePermission(Permission.Account.Manage)]
public class CreateAccount : ICreateAccount
{
    private readonly AccountsDbContext _context;
    private readonly ICreateNewAccountNumber _createNewAccountNumber;
    private readonly ISecurityContext _securityContext;
    private readonly ITopicProducer<Null, AccountEnvelop> _producer;

    public CreateAccount(AccountsDbContext context, ICreateNewAccountNumber createNewAccountNumber, ISecurityContext securityContext,
        // todo : use domain events 
        ITopicProducer<Null, AccountEnvelop> producer)
    {
        _context = context;
        _createNewAccountNumber = createNewAccountNumber;
        _securityContext = securityContext;
        _producer = producer;
    }

    public async Task<Result<CreateAccountResult>> Execute(CreateAccountRequest input, CancellationToken ct)
    {
        var currency = await _context.Currencies.FindAsync([ input.CurrencyCode ], ct);
        if (currency is null) return new DomainErrors.Currency.NotFound(input.CurrencyCode);

        var newAccountNumberArgs = new NewAccountNumberArgs(input.AccountType, Domain.BankInfo.InDuckTorBankCode, currency, input.PlannedExpiration);
        var accountNumber = await _createNewAccountNumber.Execute(newAccountNumberArgs, ct);

        var account = new Domain.Account
        {
            Number = accountNumber,
            Currency = currency,
            CurrencyCode = currency.Code,
            Type = input.AccountType,
            CreatedBy = _securityContext.Currant.Id,
            OwnerId = input.ForUserId,
            CustomComment = input.CustomComment,
            BankCode = Domain.BankInfo.InDuckTorBankCode,
            GrantedUsers = [ new GrantedAccountUser(input.ForUserId, AllAccountActions) ]
        };

        _context.Add(account);
        await _context.SaveChangesAsync(ct);
        
        await _producer.Produce(null!, account.ToCreatedEventEnvelop(), ct);

        return new CreateAccountResult(account.Number);
    }

    private static readonly AccountAction[] AllAccountActions = [ AccountAction.Withdraw, AccountAction.Freeze, AccountAction.ReadOperations, AccountAction.Close ];
}