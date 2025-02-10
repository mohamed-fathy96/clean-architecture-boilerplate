using Microsoft.OpenApi.Any;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Presentation.Infrastructure.Swagger;

public class TimeSpanFormatSchemaFilter : ISchemaFilter
{
    public void Apply(OpenApiSchema schema, SchemaFilterContext context)
    {
        if (context.Type == typeof(TimeSpan))
        {
            schema.Type = "string";
            schema.Format = "time";
            schema.Example = new OpenApiString("17:00:00");
            schema.Extensions.Add("TimeOfDay", new OpenApiString("hh:mm:ss"));
        }
    }
}
