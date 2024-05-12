using Google.Protobuf.WellKnownTypes;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Domain;
using AccountAction = InDuckTor.Account.Contracts.Public.AccountAction;
using AccountState = InDuckTor.Account.Contracts.Public.AccountState;
using AccountType = InDuckTor.Account.Contracts.Public.AccountType;
using GrantedAccountUser = InDuckTor.Account.Contracts.Public.GrantedAccountUser;
using TransactionStatus = InDuckTor.Account.Contracts.Public.TransactionStatus;
using TransactionType = InDuckTor.Account.Contracts.Public.TransactionType;

namespace InDuckTor.Account.Features.Mapping;

public static class EventsMapping
{
    public static AccountEnvelop ToCreatedEventEnvelop(this Domain.Account account)
        => new()
        {
            CreatedAt = account.CreatedAt.ToTimestamp(),
            AccountCreated = new AccountCreated
            {
                Type = (AccountType)account.Type,
                State = (AccountState)account.State,
                AccountNumber = account.Number,
                CurrencyCode = account.CurrencyCode,
                GrantedUsers = { account.GrantedUsers.Select(user => new GrantedAccountUser { Id = user.Id, Actions = { user.Actions.Select(action => (AccountAction)action) } }) },
                OwnerId = account.OwnerId,
                CreatedById = account.CreatedBy
            }
        };

    public static AccountEnvelop ToStateChangedEventEnvelop(this Domain.Account account, int changedBy)
        => new()
        {
            CreatedAt = account.CreatedAt.ToTimestamp(),
            AccountStateChanged = new()
            {
                Type = (AccountType)account.Type,
                State = (AccountState)account.State,
                AccountNumber = account.Number,
                ChangedById = changedBy,
                OwnerId = account.OwnerId
            }
        };

    public static TransactionEnvelop ToStartedEventEnvelop(this Transaction transaction)
        => new()
        {
            CreatedAt = transaction.StartedAt.ToTimestamp(),
            TransactionStarted = new()
            {
                Id = transaction.Id,
                Type = (TransactionType)transaction.Type,
                Status = (TransactionStatus)transaction.Status,
                DepositOn = transaction.DepositOn?.ToTransactionEnvelopTarget(),
                WithdrawFrom = transaction.WithdrawFrom?.ToTransactionEnvelopTarget()
            }
        };

    public static TransactionStarted.Types.Target ToTransactionEnvelopTarget(this TransactionTarget target)
        => new()
        {
            AccountNumber = target.AccountNumber,
            BankCode = target.BankCode,
            CurrencyCode = target.CurrencyCode,
            Amount = target.Amount,
            AccountOwnerId = target.Account?.OwnerId
        };

    public static TransactionEnvelop ToFinishedEventEnvelop(this Transaction transaction)
        => new()
        {
            CreatedAt = transaction.StartedAt.ToTimestamp(),
            TransactionFinished = new()
            {
                Id = transaction.Id,
                Status = (TransactionStatus)transaction.Status,
                Type = (TransactionType)transaction.Type
            }
        };
}