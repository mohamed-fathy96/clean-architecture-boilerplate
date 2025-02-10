using System.Reflection;
using CleanArchitecture.Application.Common.Behaviours;
using CleanArchitecture.Application.Models;
using CleanArchitecture.Application.Services.JwtServiceProvider;
using FirebaseAdmin;
using FluentValidation;
using Google.Apis.Auth.OAuth2;
using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Application;

public static class Bootstrapper
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddAutoMapper(Assembly.GetExecutingAssembly());

        services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(Assembly.GetExecutingAssembly());
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(UnhandledExceptionBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(AuthorizationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(ValidationBehaviour<,>));
            cfg.AddBehavior(typeof(IPipelineBehavior<,>), typeof(PerformanceBehaviour<,>));
        });

        services.AddHttpContextAccessor();

        // Hangfire
        // services.AddHangfire(x =>
        // {
        //     x.UsePostgreSqlStorage(opt =>
        //     {
        //         opt.UseNpgsqlConnection(configuration.GetConnectionString("Hangfire"));
        //     });
        // });
        //
        // services.AddHangfireServer(_ =>
        // {
        //     // Add options if needed
        // });
        
        // Identity Services

        services.Configure<JwtOptions>(configuration.GetSection("JwtOptions"));
        services.ConfigureOptions<JwtBearerOptionsSetup>();
        services.AddScoped<IJwtServiceProvider, JwtServiceProvider>();

        return services;
    }
}
