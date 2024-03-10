using FluentResults;
using InDuckTor.Account.Domain;
using InDuckTor.Account.Features.Models;
using InDuckTor.Shared.Strategies;

namespace InDuckTor.Account.Features.Account.CreateAccount;

/// <param name="ForUserId">Пользователь для которого создаётся счёт, владелец</param>
/// <param name="AccountType"></param>
/// <param name="CurrencyCode"></param>
/// <param name="CustomComment">Комментарий\записка назначении счёта при создании</param>
public record CreateAccountRequest(int ForUserId, AccountType AccountType, string CurrencyCode, DateTime? PlannedExpiration, string? CustomComment);

public interface ICreateAccount : ICommand<CreateAccountRequest, Result<CreateAccountResult>>;