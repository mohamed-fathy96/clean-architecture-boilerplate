using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Application.Common;

public static class AppHttpContext
{
    private static IHttpContextAccessor _httpContextAccessor;

    public static void Configure(IHttpContextAccessor httpContextAccessor)
    {
        _httpContextAccessor = httpContextAccessor;
    }

    public static HttpContext Current => _httpContextAccessor?.HttpContext;
}
