using System.Text;
using Confluent.Kafka;
using InDuckTor.Account.Contracts.Public;
using InDuckTor.Shared.Kafka;

namespace InDuckTor.Account.KafkaClient;

public class AccountCommandKeySerializer : ISerializer<AccountCommandKey>
{
    public byte[] Serialize(AccountCommandKey data, SerializationContext context)
    {
        if (TryCreateFromEnvelop(data.AssociatedEnvelop, out var bytes)) return bytes;

        return context.Headers.TryGetMessageId(out var messageId) ? messageId.ToByteArray() : Guid.NewGuid().ToByteArray();
    }

    private static bool TryCreateFromEnvelop(AccountCommandEnvelop envelop, out byte[] bytes)
    {
        bytes = envelop.PayloadCase switch
        {
            AccountCommandEnvelop.PayloadOneofCase.CreateAccount => BitConverter.GetBytes(envelop.CreateAccount.ForUserId),
            AccountCommandEnvelop.PayloadOneofCase.CloseAccount => Encoding.UTF8.GetBytes(envelop.CloseAccount.AccountNumber),
            AccountCommandEnvelop.PayloadOneofCase.FreezeAccount => Encoding.UTF8.GetBytes(envelop.FreezeAccount.AccountNumber),

            AccountCommandEnvelop.PayloadOneofCase.OpenTransaction
                => ConcatStringsDataStable(
                    envelop.OpenTransaction.NewTransaction.DepositOn?.AccountNumber ?? string.Empty,
                    envelop.OpenTransaction.NewTransaction.WithdrawFrom?.AccountNumber ?? string.Empty),
            AccountCommandEnvelop.PayloadOneofCase.CommitTransaction => BitConverter.GetBytes(envelop.CommitTransaction.TransactionId),
            AccountCommandEnvelop.PayloadOneofCase.CancelTransaction => BitConverter.GetBytes(envelop.CancelTransaction.TransactionId),
            AccountCommandEnvelop.PayloadOneofCase.None or _ => Array.Empty<byte>()
        };

        return bytes.Length > 0;
    }

    private static byte[] ConcatStringsDataStable(string s1, string s2)
    {
        var maxSize = Encoding.UTF8.GetMaxByteCount(s1.Length + s2.Length);
        int bytesWritten = 0;
        var bytes = new byte[maxSize];
        Span<byte> bytesSpan = bytes;

        if (string.Compare(s1, s2, StringComparison.InvariantCulture) > 0)
        {
            bytesWritten += Encoding.UTF8.GetBytes(s1, bytesSpan);
            bytesWritten += Encoding.UTF8.GetBytes(s2, bytesSpan[bytesWritten..]);
        }
        else
        {
            bytesWritten += Encoding.UTF8.GetBytes(s2, bytesSpan);
            bytesWritten += Encoding.UTF8.GetBytes(s1, bytesSpan[bytesWritten..]);
        }

        return bytesWritten == maxSize ? bytes : bytesSpan[..bytesWritten].ToArray();
    }
}