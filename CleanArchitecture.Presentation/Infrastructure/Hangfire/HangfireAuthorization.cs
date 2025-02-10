using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Hangfire.Dashboard;

namespace CleanArchitecture.Presentation.Infrastructure.Hangfire;

public class HangfireAuthorization : IDashboardAuthorizationFilter
{
    private readonly IConfiguration _config;

    public HangfireAuthorization(IConfiguration config)
    {
        _config = config;
    }
    public bool Authorize(DashboardContext dashboardContext)
    {
        var context = dashboardContext.GetHttpContext();

        string authHeader = context.Request.Headers["Authorization"];
        if (authHeader != null && authHeader.StartsWith("Basic "))
        {
            var header = AuthenticationHeaderValue.Parse(authHeader);
            if (header.Parameter != null)
            {
                var validUserName = _config.GetSection("SwaggerCredentials")["UserName"];
                var validPassword = _config.GetSection("SwaggerCredentials")["Password"];
                var bytes = Convert.FromBase64String(header.Parameter);
                var credentials = Encoding.UTF8.GetString(bytes).Split(':');
                var username = credentials[0];
                var password = credentials[1];
                if (username.Equals(validUserName)
                    && password.Equals(validPassword))
                {
                    return true;
                }
            }
        }

        context.Response.Headers["WWW-Authenticate"] = "Basic";
        context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        return false;
    }
}
