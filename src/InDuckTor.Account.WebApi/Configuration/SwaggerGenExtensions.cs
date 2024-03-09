using System.Xml.Linq;
using System.Xml.XPath;
using InDuckTor.Shared.Configuration;

namespace InDuckTor.Account.WebApi.Configuration;

public static class SwaggerGenExtensions
{
    public static IServiceCollection AddAccountSwaggerGen(this IServiceCollection serviceCollection)
    {
        return serviceCollection.AddSwaggerGen(options =>
        {
            options.ConfigureJwtAuth();
            options.ConfigureEnumMemberValues();
            options.CustomSchemaIds(ComposeNameWithDeclaringType);
            
            var dir = new DirectoryInfo(AppContext.BaseDirectory);
            foreach (var fi in dir.EnumerateFiles("*.xml"))
            {
                var doc = XDocument.Load(fi.FullName);
                options.IncludeXmlComments(() => new XPathDocument(doc.CreateReader()), true);
            }
        });

        string ComposeNameWithDeclaringType(Type type)
            => type.DeclaringType is null
                ? type.Name
                : string.Join('.', ComposeNameWithDeclaringType(type.DeclaringType), type.Name);
    }
}