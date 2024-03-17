using InDuckTor.Shared.Models;

namespace InDuckTor.Account.Domain;

public static class DomainErrors
{
    public static class Currency
    {
        public class NotFound(string currencyCode) : Errors.NotFound($"Код валюты {currencyCode} не найден");
    }

    public static class Transaction
    {
        public class NotFound(long transactionId) : Errors.NotFound($"Трансакция {transactionId} не найдена");
    }

    public static class Account
    {
        public class NotFound(string accountNumber) : Errors.NotFound($"Счёт {accountNumber} не найден");

        public class NotEmpty(string accountNumber) : Errors.Conflict($"На счёте {accountNumber} есть остаток средств, необходимо опустошить счёт для совершения действия");

        public class NotEnoughFunds() : Errors.Conflict("Недостаточно средств");
    }
}