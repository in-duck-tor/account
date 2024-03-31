using System.Text;
using Microsoft.Extensions.DependencyInjection;

namespace InDuckTor.Account.Cbr.Integration;

public static class DependencyRegistration
{
    public static IServiceCollection AddCbrIntegration(this IServiceCollection services)
    {
        // for encoding="windows-1251"
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        
        services.AddHttpClient<ICbrClient, CbrHttpClient>((provider, client) =>
        {
            client.BaseAddress = new Uri("http://cbr.ru");
        });
        
        return services;
    }
}