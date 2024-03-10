using System.Linq.Expressions;

namespace InDuckTor.Account.Domain;

public static partial  class Specifications
{
    public static class Transaction
    {
        public static Expression<Func<Domain.Transaction, bool>> RelatedToAccount(AccountNumber accountNumber)
        {
            return x => x.DepositOn.AccountNumber == accountNumber || x.WithdrawFrom.AccountNumber == accountNumber;
        }
    }
}