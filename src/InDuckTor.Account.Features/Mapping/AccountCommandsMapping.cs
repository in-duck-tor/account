using InDuckTor.Account.Contracts.Public;
using InDuckTor.Account.Features.Account.CreateAccount;
using InDuckTor.Account.Features.Models;
using InDuckTor.Account.Features.PaymentAccount;
using InDuckTor.Account.Features.Transactions;
using InDuckTor.Shared.Protobuf;
using Mapster;
using CreateAccount = InDuckTor.Account.Contracts.Public.CreateAccount;
using FreezeAccount = InDuckTor.Account.Contracts.Public.FreezeAccount;
using OpenTransaction = InDuckTor.Account.Contracts.Public.OpenTransaction;

namespace InDuckTor.Account.Features.Mapping;

public class AccountCommandsMapping : IRegister
{
    public void Register(TypeAdapterConfig config)
    {
        config.NewConfig<DecimalValue, decimal>().MapWith(value => (decimal)value);
        config.NewConfig<decimal, DecimalValue>().MapWith(value =>  (DecimalValue)value);
        
        // todo test 
        config.NewConfig<CreateAccount, CreateAccountRequest>().TwoWays();
        config.NewConfig<FreezeAccount, FreezeAccountRequest>().TwoWays();
        // config.NewConfig<CloseAccount, AccountNumber>();
        
        config.NewConfig<NewTransactionRequest, OpenTransaction>()
            .Map(d => d.NewTransaction, s => s)
            .Map(d => d.ExecuteImmediate, s => true)
            .Map(d => d.RequestedTransactionTtl, s => null as double?);
        config.NewConfig<OpenTransaction, NewTransactionRequest>()
            .Map(d => d, s => s.NewTransaction);
        
        config.NewConfig<OpenTransaction, OpenTransactionRequest>().TwoWays();
        // config.NewConfig<CommitTransaction, long>();
        // config.NewConfig<CancelTransaction, long>();
    }
}