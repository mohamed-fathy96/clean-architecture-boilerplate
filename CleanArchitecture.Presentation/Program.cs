using System.Globalization;
using CleanArchitecture.Application;
using CleanArchitecture.Application.Common;
using CleanArchitecture.Infrastructure;
using CleanArchitecture.Presentation;
using CleanArchitecture.Presentation.Infrastructure;
using CleanArchitecture.Presentation.Infrastructure.Hangfire;
using CleanArchitecture.Presentation.Middlewares;
using Hangfire;
using Microsoft.AspNetCore.Localization;
using Portrage.Presentation.Middlewares;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

builder.WebHost.UseKestrel(options => { options.Limits.MaxRequestBodySize = null; });

DotNetEnv.Env.Load();

builder.Services
    .AddApplicationDbContext(builder.Configuration)
    .AddUnitOfWork(builder.Configuration)
    .AddApplicationServices(builder.Configuration)
    .AddWebServices();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Host.AddSerilog();

var app = builder.Build();

var context = app.Services.GetRequiredService<IHttpContextAccessor>();
AppHttpContext.Configure(context);

if (!app.Environment.IsProduction())
{
    app.UseMiddleware<SwaggerAuthMiddleware>();
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.DocExpansion(Swashbuckle.AspNetCore.SwaggerUI.DocExpansion.None);
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "Clean Architecture Api");
        c.DocumentTitle = "Clean Architecture API documentation";
    }); 
}

app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.UseExceptionHandler(_ => { });
app.UseCors("CorsPolicy");
app.MapEndpoints();
app.UseSerilogRequestLogging();
app.UseRequestLocalization(o => { o.DefaultRequestCulture = new RequestCulture(new CultureInfo("en")); });
app.UseMiddleware<LocalizationMiddleware>();


app.UseWebSockets(options: new WebSocketOptions
{
    KeepAliveInterval = TimeSpan.FromMinutes(15)
});

// app.UseHangfireDashboard("/dashboard", options: new DashboardOptions
// {
//     Authorization = new[] { new HangfireAuthorization(app.Configuration) },
// });

app.Run();