using CleanArchitecture.Presentation.Infrastructure;
using MediatR;

namespace CleanArchitecture.Presentation.Endpoints;

public class TestEndpoints : EndpointGroupBase
{
    public override void Map(WebApplication app)
    {
        var group = app.MapGroup(this, "test", "Test");

        group.MapGet("/", Test)
            .WithName(nameof(Test))
            .WithSummary("Test")
            .Produces(StatusCodes.Status200OK);
    }

    private static async Task<IResult> Test(ISender sender)
    {
        return await Task.FromResult(TypedResults.Ok(new {Message = "Test succeeded"}));
    }
}
