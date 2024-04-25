using System.Reflection;
using Mapster;

namespace InDuckTor.Account.WebApi.Configuration;

public static class MapsterConfiguration
{
    /// <param name="assemblies">Сборки, которые нужно просканировать</param>
    public static void ConfigureMapster(params Assembly[] assemblies)
    {
        TypeAdapterConfig.GlobalSettings.Scan(assemblies);
        TypeAdapterConfig.GlobalSettings.Compile();
    }
}