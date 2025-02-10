using System.Net;
using System.Net.Http.Headers;
using System.Text;

namespace CleanArchitecture.Presentation.Middlewares;

public class SwaggerAuthMiddleware(RequestDelegate next, IConfiguration config)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/swagger"))
        {
            string authHeader = context.Request.Headers["Authorization"];
            if (authHeader != null && authHeader.StartsWith("Basic "))
            {
                var header = AuthenticationHeaderValue.Parse(authHeader);
                if (header.Parameter != null)
                {
                    var validUserName = config.GetSection("SwaggerCredentials")["UserName"];
                    var validPassword = config.GetSection("SwaggerCredentials")["Password"];
                    var bytes = Convert.FromBase64String(header.Parameter);
                    var credentials = Encoding.UTF8.GetString(bytes).Split(':');
                    var username = credentials[0];
                    var password = credentials[1];
                    if (username.Equals(validUserName)
                        && password.Equals(validPassword))
                    {
                        await next.Invoke(context).ConfigureAwait(false);
                        return;
                    }
                }
            }

            context.Response.Headers["WWW-Authenticate"] = "Basic";
            context.Response.StatusCode = (int)HttpStatusCode.Unauthorized;
        }
        else
        {
            await next.Invoke(context).ConfigureAwait(false);
        }
    }
}