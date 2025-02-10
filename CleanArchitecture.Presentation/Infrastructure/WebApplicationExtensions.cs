using System.Reflection;
using CleanArchitecture.Domain.Shared;
using CleanArchitecture.Presentation.Infrastructure.Filters;

namespace CleanArchitecture.Presentation.Infrastructure;

public static class WebApplicationExtensions
{
    public static RouteGroupBuilder MapGroup(this WebApplication app, EndpointGroupBase group, string route = "",
        string groupName = "")
    {
        groupName = groupName.IsEmpty() ? group.GetType().Name : groupName;
        route = route.IsEmpty() ? groupName : route;

        return app
            .MapGroup($"/api/{route}")
            .WithGroupName(groupName)
            .WithTags(groupName)
            .WithOpenApi()
            .AddEndpointFilter<LocalizationActionFilter>();
    }

    public static WebApplication MapEndpoints(this WebApplication app)
    {
        var endpointGroupType = typeof(EndpointGroupBase);

        var assembly = Assembly.GetExecutingAssembly();

        var endpointGroupTypes = assembly.GetExportedTypes()
            .Where(t => t.IsSubclassOf(endpointGroupType));

        foreach (var type in endpointGroupTypes)
        {
            if (Activator.CreateInstance(type) is EndpointGroupBase instance)
            {
                instance.Map(app);
            }
        }

        return app;
    }
}
