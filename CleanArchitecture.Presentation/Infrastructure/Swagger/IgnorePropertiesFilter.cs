using System.Reflection;
using CleanArchitecture.Domain.Shared;
using Microsoft.OpenApi.Models;
using Portrage.Domain.Shared.Attributes;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Presentation.Infrastructure.Swagger;

public class SwaggerExcludeFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema?.Properties == null || context.Type == null)
            return;

        var excludedProperties = context.Type.GetProperties()
            .Where(t =>
                t.GetCustomAttribute<SwaggerExcludeAttribute>()
                != null);

        foreach (var excludedProperty in excludedProperties)
        {
            if (schema.Properties.ContainsKey(excludedProperty.Name.ToCamelCase()))
                schema.Properties.Remove(excludedProperty.Name.ToCamelCase());
        }
    }
}
