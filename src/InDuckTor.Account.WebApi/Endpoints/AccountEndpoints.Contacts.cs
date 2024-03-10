using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;

namespace InDuckTor.Account.WebApi.Endpoints;





public class OpenTransactionRequest
{
    public required NewTransactionRequest NewTransaction { get; set; }
    public bool ExecuteImmediate { get; set; } = false;
    public TimeSpan? RequestedTransactionTtl { get; set; }
}

public record OpenTransactionResult(long TransactionId, TransactionType TransactionType, TransactionStatus Status, TimeSpan TransactionTtl);