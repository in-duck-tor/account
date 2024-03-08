namespace InDuckTor.Account.Domain;

/// <summary>
/// Резервация денежных средств на счёту
/// </summary>
public class FundsReservation
{
    public long Id { get; set; }

    public required decimal Amount { get; init; }

    public required AccountNumber AccountNumber { get; init; }

    public int? TransactionId { get; set; }
}