using InDuckTor.Account.Domain;

namespace InDuckTor.Account.WebApi.Contracts;

public record MakeTransactionRequest(TransactionType Type, MakeTransactionRequest.Target? DepositOn, MakeTransactionRequest.Target? WithdrawFrom)
{
    public record Target(decimal Amount, string AccountNumber, string BankCode);
}