using System.Xml.Linq;
using System.Xml.XPath;
using InDuckTor.Account.Domain;
using InDuckTor.Shared.Configuration.Swagger;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

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
            options.SchemaFilter<AccountNumberSchemaFilter>();
            options.SchemaFilter<BankCodeSchemaFilter>();
            
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
    
    private class AccountNumberSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(AccountNumber)) return;
            schema.Type = "string";
            schema.Description = "Номер счёта из 20 цифр";
        }
    }
    
    private class BankCodeSchemaFilter : ISchemaFilter
    {
        public void Apply(OpenApiSchema schema, SchemaFilterContext context)
        {
            if (context.Type != typeof(BankCode)) return;
            schema.Type = "string";
            schema.Description = "БИК. 9-ти значный уникальный код банка";
        }
    }
}