using CleanArchitecture.Application.Common;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Services.Localization;
using CleanArchitecture.Domain.Shared;
using CleanArchitecture.Domain.Shared.Dtos;
using Microsoft.Extensions.Localization;

namespace CleanArchitecture.Presentation.Infrastructure;

public abstract class EndpointGroupBase
{
    public abstract void Map(WebApplication app);

    protected static IResult AsOk(IStringLocalizer<EndpointGroupBase> localizer, string message = "")
    {
        message = message.IsEmpty() ? localizer[ResourceKeys.GenericAsOkMessage] : message;
        return TypedResults.Ok(new ApiResponse
        {
            Status = 200,
            Title = "Success",
            Detail = message,
            Errors = null
        });
    }

    protected static IResult AsBadRequest(IStringLocalizer<EndpointGroupBase> localizer, string message = "")
    {
        message = message.IsEmpty() ? localizer[ResourceKeys.GenericAsBadRequestMessage] : message;
        return TypedResults.BadRequest(new ApiResponse
        {
            Status = 400,
            Title = "Failure",
            Detail = message,
            Errors = null
        });
    }

    protected static int GetUserId()
    {
        var userId = AppHttpContext.Current.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value ?? "0";
        return !int.TryParse(userId, out var result) ? 0 : result;
    }

    protected static string GetUserName()
    {
        var userName = AppHttpContext.Current.User.Claims.FirstOrDefault(x => x.Type == "name")?.Value ?? "";
        return userName;
    }

    protected static int GetCountryId()
    {
        var countryId = AppHttpContext.Current.Request.Headers["x-country-id"];

        if (countryId.IsEmpty())
            return 1; // Kuwait by default

        return !int.TryParse(countryId, out var result) ? 1 : result;
    }

    protected static string GetLanguage()
    {
        var lang = AppHttpContext.Current.Request.Headers["x-lang"];

        if (lang.IsEmpty())
            return "en";

        return lang;
    }
    
    public static string GetAppVersion()
    {
        var appVersion = AppHttpContext.Current.Request.Headers["x-app-version"];
        return appVersion.IsEmpty() ? string.Empty : appVersion;
    }
    public static string GetUniqueId()
    {
        return AppHttpContext.Current.Request.Headers["x-unique-id"];
    }
    
    protected static int GetVendorId()
    {
        var vendorId = AppHttpContext.Current.User.Claims.FirstOrDefault(x => x.Type == "vendorId")?.Value ?? "0";
        return !int.TryParse(vendorId, out var result) ? 0 : result;
    }

    public static (int userId, string uniqueId) GetAndValidateUserIdAndUniqueId()
    {
        var uniqueId = GetUniqueId();
        var userId = GetUserId();

        if (uniqueId.IsEmpty() && userId == 0)
            throw new UnauthorizedBusinessException();

        return (userId, uniqueId);
    }
}
