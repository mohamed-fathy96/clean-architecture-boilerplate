using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;

namespace CleanArchitecture.Presentation.Infrastructure.Swagger;

public class CustomHeaderOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        operation.Parameters ??= new List<OpenApiParameter>();

        // Lang header

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-lang",
            In = ParameterLocation.Header,
            Description = "Define the language for localized user experience. Accepted values are (EN & AR). Default value is EN",
            Required = false,
            Deprecated = false,
            AllowEmptyValue = true,
            Style = ParameterStyle.Form,
        });

        // Country Id header

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-country-id",
            In = ParameterLocation.Header,
            Description = "Define the user's country Id",
            Required = false,
            Deprecated = false,
            AllowEmptyValue = true,
            Style = ParameterStyle.Form,
        });

        // Device Type Header

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-device-type",
            In = ParameterLocation.Header,
            Description = "Define the user's device type, Android = 0, IOS = 1",
            Required = false,
            Deprecated = false,
            AllowEmptyValue = true,
            Style = ParameterStyle.Form,
        });

        // Unique Id Header

        operation.Parameters.Add(new OpenApiParameter
        {
            Name = "x-unique-id",
            In = ParameterLocation.Header,
            Description = "Define the user's unique Id (used for guest flow)",
            Required = false,
            Deprecated = false,
            AllowEmptyValue = true,
            Style = ParameterStyle.Form,
        });
    }
}
