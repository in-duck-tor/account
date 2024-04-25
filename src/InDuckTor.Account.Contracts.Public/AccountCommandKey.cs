namespace InDuckTor.Account.Contracts.Public;

public readonly struct AccountCommandKey
{
    public readonly AccountCommandEnvelop AssociatedEnvelop;

    public AccountCommandKey(AccountCommandEnvelop associatedEnvelop) => AssociatedEnvelop = associatedEnvelop;
}