using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.PaymentAccount;
using InDuckTor.Account.Features.Transactions;
using Mapster;
using CreateAccount = InDuckTor.Account.Features.Account.CreateAccount.CreateAccount;
using FreezeAccount = InDuckTor.Account.Contracts.Public.FreezeAccount;
using OpenTransaction = InDuckTor.Account.Contracts.Public.OpenTransaction;

namespace InDuckTor.Account.Features.Mapping;

public class AccountCommandsMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        // todo test 
        config.NewConfig<CreateAccount, CreateAccountRequest>().TwoWays();
        config.NewConfig<FreezeAccount, FreezeAccountRequest>().TwoWays();
        // config.NewConfig<CloseAccount, AccountNumber>();
        
        config.NewConfig<OpenTransaction, OpenTransactionRequest>().TwoWays();
        // config.NewConfig<CommitTransaction, long>();
        // config.NewConfig<CancelTransaction, long>();
    }
}