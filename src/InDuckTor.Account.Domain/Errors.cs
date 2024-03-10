using FluentResults;

namespace InDuckTor.Account.Domain;

public static class Errors
{
    public class Forbidden(string? message = null) : Error(message);

    public class NotFound(string message) : Error(message);

    public class Conflict(string message) : Error(message);

    public class InvalidInput(string message) : Error(message)
    {
        public void AddInvalidField(string fieldName, string message)
        {
            if (Metadata.TryGetValue(fieldName, out var value) && value is List<string> validationMessages)
            {
                validationMessages.Add(message);
                return;
            }

            Metadata.Add(fieldName, new List<string> { message });
        }

        public Dictionary<string, string[]> ProduceFieldsErrors()
        {
            var errorsMap = new Dictionary<string, string[]>(Metadata.Count);
            foreach (var pair in Metadata)
            {
                if (Metadata[pair.Key] is not List<string> validationMessages) continue;
                errorsMap.Add(pair.Key, validationMessages.ToArray());
            }

            return errorsMap;
        }
    }

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