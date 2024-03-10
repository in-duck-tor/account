using FluentResults;
using InDuckTor.Account.Features.Models;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Account.GetAccountTransactions;

public record GetAccountTransactionsParams(string AccountNumber, int? Take, int? Skip);

public interface IGetAccountTransactions : IQuery<GetAccountTransactionsParams, Result<TransactionDto[]>>;