using System.Collections;
using System.Globalization;
using System.Reflection;

namespace CleanArchitecture.Presentation.Infrastructure.Filters;

public class LocalizationActionFilter : IEndpointFilter
{
    public async ValueTask<object> InvokeAsync(EndpointFilterInvocationContext context, EndpointFilterDelegate next)
    {
        dynamic result = await next(context);

        if (result is not IStatusCodeHttpResult { StatusCode: 200 })
            return result;

        var value = result.Value;
        var culture = CultureInfo.CurrentCulture.Name;

        ModifyObject(value, culture);

        return result;
    }

    private void ModifyObject(object obj, string culture)
    {
        if (obj is null)
            return;

        var type = obj.GetType();

        if (typeof(IEnumerable).IsAssignableFrom(type))
        {
            if (obj is not IEnumerable enumerable)
                return;

            foreach (var item in enumerable)
                ModifyObject(item, culture);
        }
        else
        {
            var properties = obj.GetType().GetProperties();
            foreach (var property in properties)
            {
                if (property.PropertyType == typeof(string) &&
                    (property.Name.EndsWith("En") || property.Name.EndsWith("Ar")))
                {
                    ModifyLocalizedProperty(obj, property, culture);
                }
                else if (property.PropertyType.IsClass && property.PropertyType != typeof(string))
                {
                    var nestedObject = property.GetValue(obj);
                    ModifyObject(nestedObject, culture);
                }
                else if (typeof(IEnumerable).IsAssignableFrom(property.PropertyType))
                {
                    if (property.GetValue(obj) is not IEnumerable enumerable)
                        continue;

                    foreach (var item in enumerable)
                        ModifyObject(item, culture);
                }
            }
        }
    }

    private static void ModifyLocalizedProperty(object obj, PropertyInfo property, string culture)
    {
        string postfix = culture.Equals("ar", StringComparison.OrdinalIgnoreCase) ? "Ar" : "En";
        string baseName = property.Name.Remove(property.Name.Length - 2);

        var baseProperty = obj.GetType().GetProperty(baseName);

        if (baseProperty == null || baseProperty.PropertyType != typeof(string) || baseProperty.GetValue(obj) != null)
            return;

        if (!property.Name.EndsWith(postfix))
            return;

        var value = property.GetValue(obj) as string;
        baseProperty.SetValue(obj, value);
    }
}
