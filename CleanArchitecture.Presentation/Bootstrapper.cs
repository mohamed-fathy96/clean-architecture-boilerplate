using System.Reflection;
using System.Threading.RateLimiting;
using CleanArchitecture.Application.Common.Exceptions;
using CleanArchitecture.Application.Services.Localization;
using CleanArchitecture.Presentation.Infrastructure;
using CleanArchitecture.Presentation.Infrastructure.Swagger;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Localization;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using NpgsqlTypes;
using Serilog;
using Serilog.Events;
using Serilog.Sinks.PostgreSQL;
using Swashbuckle.AspNetCore.Filters;
using ZymLabs.NSwag.FluentValidation;

namespace CleanArchitecture.Presentation;

public static class Bootstrapper
{
    public static IServiceCollection AddWebServices(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen();

        services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicy",
                b => b
                    .SetIsOriginAllowed(_ => true)
                    .AllowAnyMethod()
                    .AllowAnyHeader()
                    .AllowCredentials());
        });

        services.AddHttpContextAccessor();

        services.AddSwaggerGen(c =>
        {
            var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
            c.IncludeXmlComments(xmlPath);

            c.AddSecurityDefinition("oauth2", new OpenApiSecurityScheme
            {
                Description = @"JWT Authorization header using the Bearer scheme.
                    Enter 'bearer' [space] and then your token in the text input below.
                    Example: 'bearer 12345abcdef'",
                Name = "Authorization",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.ApiKey
            });

            c.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "oauth2"
                        }
                    },
                    Array.Empty<string>()
                }
            });

            c.OperationFilter<SecurityRequirementsOperationFilter>();
            c.OperationFilter<CustomHeaderOperationFilter>();

            c.SchemaFilter<TimeSpanFormatSchemaFilter>();
            c.SchemaFilter<SwaggerExcludeFilter>();

            c.TagActionsBy(api =>
            {
                if (api.GroupName != null)
                {
                    return new[] { api.GroupName };
                }

                var routePattern = api.RelativePath;
                if (!string.IsNullOrEmpty(routePattern))
                {
                    // Extract the controller name from the route pattern
                    var controllerName =
                        routePattern.Split('/', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault();
                    if (!string.IsNullOrEmpty(controllerName))
                    {
                        return new[] { controllerName };
                    }
                }

                throw new InvalidOperationException("Unable to determine tag for endpoint.");
            });

            c.DocInclusionPredicate((_, _) => true);
        });

        services.AddScoped(provider =>
        {
            var validationRules = provider.GetService<IEnumerable<FluentValidationRule>>();
            var loggerFactory = provider.GetService<ILoggerFactory>();

            return new FluentValidationSchemaProcessor(provider, validationRules, loggerFactory);
        });

        services.AddEndpointsApiExplorer();

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AuthorizeHub", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("userType", "HubUser");
            });

            options.AddPolicy("AuthorizeDriver", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("userType", "Driver");
            });

            options.AddPolicy("AuthorizeVendor", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireClaim("userType", "Vendor");
            });
        });

        services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme).AddJwtBearer();
        services.AddSingleton<IExceptionHandler, ExceptionHandler>(provider =>
        {
            var environment = provider.GetRequiredService<IWebHostEnvironment>();
            var logger = provider.GetRequiredService<ILogger<ExceptionHandler>>();
            return new ExceptionHandler(environment, logger);
        });

        services.AddRateLimiter(options =>
        {
            options.AddPolicy("user-ip", httpContext =>
                RateLimitPartition.GetFixedWindowLimiter(httpContext.Request.Headers["X-Real-IP"],
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = true,
                        PermitLimit = 1,
                        Window = TimeSpan.FromMinutes(2)
                    }));

            options.OnRejected = async (context, token) =>
            {
                context.HttpContext.Response.StatusCode = 429;

                var responseObject = new
                {
                    Status = 429,
                    Title = $"Too many requests {context.HttpContext.Request.Headers["X-Real-IP"]}",
                    Detail = context.Lease.TryGetMetadata(MetadataName.RetryAfter, out var retryAfter)
                        ? $"Please try again after {retryAfter.TotalMinutes} minute(s)."
                        : "Please try again later.",
                    Type = typeof(BadRequestException)
                };

                var jsonResponse = JsonConvert.SerializeObject(responseObject,
                    new JsonSerializerSettings { ContractResolver = new CamelCasePropertyNamesContractResolver() });

                context.HttpContext.Response.ContentType = "application/json";
                await context.HttpContext.Response.WriteAsync(jsonResponse, token);
            };
        });

        // Localization

        services.AddLocalization();
        services.AddDistributedMemoryCache();
        services.AddSingleton<IStringLocalizerFactory, JsonStringLocalizerFactory>();

        return services;
    }

    public static void AddSerilog(this IHostBuilder builder)
    {
        builder.UseSerilog((context, config) =>
        {
            var connection = context.Configuration.GetConnectionString("LoggingConnectionString");

            var tableName = context.HostingEnvironment.IsProduction()
                ? "production_logs"
                : "staging_logs";

            var minimumLevel = context.HostingEnvironment.IsProduction()
                ? LogEventLevel.Error
                : LogEventLevel.Warning;


            IDictionary<string, ColumnWriterBase> columnWriters = new Dictionary<string, ColumnWriterBase>
            {
                { "message", new RenderedMessageColumnWriter() },
                { "message_template", new MessageTemplateColumnWriter() },
                { "level", new LevelColumnWriter(true, NpgsqlDbType.Varchar) },
                { "raise_date", new TimestampColumnWriter() },
                { "exception", new ExceptionColumnWriter() },
                { "properties", new LogEventSerializedColumnWriter() },
                { "props_test", new PropertiesColumnWriter() }
            };

            config.WriteTo.Logger(lc => lc
                .Filter.ByExcluding(evt =>
                    evt.Exception is ValidationException or BadRequestException)
                .WriteTo.PostgreSQL(connection, tableName, columnWriters, minimumLevel,
                    needAutoCreateTable: true, schemaName: "public"));

            config.WriteTo.Console();
        });
    }
}
