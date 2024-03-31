using System.Text;
using System.Xml.Serialization;

namespace InDuckTor.Account.Cbr.Integration;

public interface ICbrClient
{
    public Task<ValCurs?> GetCurrencies(DateOnly dateOnly, CancellationToken cancellationToken);
}

public class CbrHttpClient : ICbrClient
{
    private readonly HttpClient _httpClient;
    private XmlSerializer _xmlSerializer;

    public CbrHttpClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _xmlSerializer = new XmlSerializer(typeof(ValCurs));
    }

    public async Task<ValCurs?> GetCurrencies(DateOnly dateOnly, CancellationToken cancellationToken)
    {
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, $"/scripts/XML_daily.asp?date_req={dateOnly:dd/MM/yyyy}");
        var httpResponseMessage = await _httpClient.SendAsync(httpRequestMessage, cancellationToken);

        return _xmlSerializer.Deserialize(
                await httpResponseMessage.Content.ReadAsStreamAsync(cancellationToken))
            as ValCurs;
    }
}