using CleanArchitecture.Domain.Interfaces;
using CleanArchitecture.Infrastructure.Common;
using CleanArchitecture.Infrastructure.Contexts;
using CleanArchitecture.Infrastructure.Interceptors;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace CleanArchitecture.Infrastructure;

public static class Bootstrapper
{
    public static IServiceCollection AddApplicationDbContext(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<ISaveChangesInterceptor, BaseEntityInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, DispatchDomainEventsInterceptor>();
        services.AddScoped<ISaveChangesInterceptor, SoftDeleteInterceptor>();
        services.AddScoped<IDbCommandInterceptor, ForUpdateInterceptor>();

        services.AddDbContext<WriteDbContext>((sp, opt) =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("WriteDbContext"),
                    op => { op.UseNetTopologySuite(); })
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseSnakeCaseNamingConvention()
                .AddInterceptors(sp.GetServices<ISaveChangesInterceptor>())
                .AddInterceptors(sp.GetServices<IDbCommandInterceptor>());
        });
        services.AddDbContext<ReadDbContext>(opt =>
        {
            opt.UseNpgsql(configuration.GetConnectionString("ReadDbContext"),
                    op =>
                    {
                        op.UseQuerySplittingBehavior(QuerySplittingBehavior.SplitQuery);
                        op.UseNetTopologySuite();
                    })
                .EnableSensitiveDataLogging()
                .EnableDetailedErrors()
                .UseQueryTrackingBehavior(QueryTrackingBehavior.NoTracking)
                .UseSnakeCaseNamingConvention();
        });
        return services;
    }

    public static IServiceCollection AddUnitOfWork(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IUnitOfWork, UnitOfWork>();
        return services;
    }
}
