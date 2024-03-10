﻿namespace InDuckTor.Account.Features.Models;

/// <param name="Amount"><b>ДЕНЬГИ</b></param>
public record NewTransactionRequest(decimal Amount, NewTransactionRequest.Target? DepositOn, NewTransactionRequest.Target? WithdrawFrom)
{
    /// <param name="BankCode">БИК</param>
    public record Target(string AccountNumber, string BankCode);
}