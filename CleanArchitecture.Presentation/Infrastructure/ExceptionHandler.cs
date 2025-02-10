using System.Security.Claims;
using CleanArchitecture.Application.Common.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;

namespace CleanArchitecture.Presentation.Infrastructure;

public class ExceptionHandler : IExceptionHandler
{
    private readonly IWebHostEnvironment _environment;
    private readonly Dictionary<Type, Func<HttpContext, Exception, Task>> _exceptionHandlers;
    private readonly ILogger<ExceptionHandler> _logger;

    public ExceptionHandler(IWebHostEnvironment environment, ILogger<ExceptionHandler> logger)
    {
        _environment = environment;
        _logger = logger;
        _exceptionHandlers = new Dictionary<Type, Func<HttpContext, Exception, Task>>
        {
            { typeof(ValidationException), HandleValidationException },
            { typeof(NotFoundException), HandleNotFoundException },
            { typeof(UnauthorizedBusinessException), HandleUnauthorizedAccessException },
            { typeof(BadRequestException), HandleBadRequestException }
        };
    }

    public async ValueTask<bool> TryHandleAsync(HttpContext httpContext, Exception exception,
        CancellationToken cancellationToken)
    {
        var exceptionType = exception.GetType();

        // If response has started do nothing

        if (httpContext.Response.HasStarted)
            return true;

        if (!_exceptionHandlers.TryGetValue(exceptionType, out var handler))
        {
            await HandleInternalServerError(httpContext, exception);
            return true;
        }

        await handler.Invoke(httpContext, exception);
        return true;
    }

    private static async Task HandleValidationException(HttpContext httpContext, Exception ex)
    {
        var exception = (ValidationException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ValidationProblemDetails(exception.Errors)
        {
            Status = StatusCodes.Status400BadRequest,
            Type = nameof(ValidationException)
        });
    }

    private static async Task HandleNotFoundException(HttpContext httpContext, Exception ex)
    {
        var exception = (NotFoundException)ex;

        httpContext.Response.StatusCode = StatusCodes.Status404NotFound;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status404NotFound,
            Type = nameof(NotFoundException),
            Title = "The specified resource was not found.",
            Detail = exception.Message
        });
    }

    private static async Task HandleUnauthorizedAccessException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status401Unauthorized;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status401Unauthorized,
            Title = "Unauthorized",
            Type = nameof(UnauthorizedAccessException),
            Detail = ""
        });
    }

    private async Task HandleBadRequestException(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status400BadRequest;

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status400BadRequest,
            Title = "Bad Request",
            Detail = $"{ex.Message}",
            Type = nameof(BadRequestException)
        });
    }

    private async Task HandleInternalServerError(HttpContext httpContext, Exception ex)
    {
        httpContext.Response.StatusCode = StatusCodes.Status500InternalServerError;

        string requestBody;

        var originalBody = httpContext.Request.Body;

        using var memoryStream = new MemoryStream();
        await originalBody.CopyToAsync(memoryStream);
        memoryStream.Seek(0, SeekOrigin.Begin);

        using (var reader = new StreamReader(memoryStream, leaveOpen: true))
        {
            requestBody = await reader.ReadToEndAsync();
        }

        memoryStream.Seek(0, SeekOrigin.Begin);
        httpContext.Request.Body = memoryStream;

        await LogError(httpContext, ex, _environment.EnvironmentName, requestBody);

        await httpContext.Response.WriteAsJsonAsync(new ProblemDetails
        {
            Status = StatusCodes.Status500InternalServerError,
            Title = "Internal Server Error",
            Detail = !_environment.IsProduction()
                ? ex.ToString()
                : "An internal server error occurred, please try again later",
            Type = nameof(Exception)
        });
    }

    private async Task LogError(HttpContext httpContext, Exception exception, string environment,
        string requestBody)
    {
        var message = new ExceptionLog
        {
            Environment = environment,
            // Get request info
            Method = httpContext.Request.Method,
            FullPath = httpContext.Request.Path.Value ?? "",
            QueryCollection = httpContext.Request.Query
        };

        if (httpContext.Request.ContentType != null &&
            httpContext.Request.ContentType.ToLower().Contains("form-data"))
        {
            try
            {
                var formCollection = await httpContext.Request.ReadFormAsync();
                message.Body = formCollection.ToDictionary(x => x.Key, y => y.Value);
            }
            catch
            {
                // ignore
            }
        }
        else if (httpContext.Request.ContentType != null &&
                 httpContext.Request.ContentType.ToLower().Contains("application/json"))
        {
            message.Body = JsonConvert.DeserializeObject(requestBody);
        }

        // serialized body
        var serializedBody = string.Empty;
        if (message.Body is not null)
        {
            serializedBody = JsonConvert.SerializeObject(message.Body, Formatting.None);
        }

        // Get user info
        message.UserId = httpContext.User.Claims.FirstOrDefault(x => x.Type == "id")?.Value ?? "";
        message.IsAuthenticated = httpContext.User.Identity?.IsAuthenticated ?? false;
        message.UserRole = httpContext.User.Claims.FirstOrDefault(x => x.Type == ClaimTypes.Role)?.Value ?? "";

        // Log Exception Details
        message.StatusCode = httpContext.Response.StatusCode;
        message.ExceptionMessage = exception.Message;
        message.Exception = exception;


        _logger.LogError("environment: {environment}\n" +
                         "method: {method}\n" +
                         "full_path: {full_path}\n" +
                         "query_collection: {query_collection}\n" +
                         "request_body: {request_body}\n" +
                         "is_authenticated: {is_authenticated}\n" +
                         "user_id: {user_id}\n" +
                         "user_role: {user_role}\n" +
                         "status_code: {status_code}\n" +
                         "exception_message: {exception_message}\n" +
                         "exception_stack_trace: {exception_stack_trace}\n" +
                         "inner_exception_message: {inner_exception_message}\n" +
                         "inner_exception_stack_trace: {inner_exception_stack_trace}\n",
            message.Environment, message.Method, message.FullPath, message.QueryCollection, serializedBody,
            message.IsAuthenticated, message.UserId, message.UserRole, message.StatusCode, message.ExceptionMessage,
            message.Exception.StackTrace, message.Exception.InnerException?.Message,
            message.Exception.InnerException?.StackTrace);
    }
}
