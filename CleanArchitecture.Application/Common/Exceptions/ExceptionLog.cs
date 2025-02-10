using Microsoft.AspNetCore.Http;

namespace CleanArchitecture.Application.Common.Exceptions;

public class ExceptionLog
{
    public string Environment { get; set; }
    public int StatusCode { get; set; }
    public string FullPath { get; set; }
    public string Method { get; set; }
    public dynamic Body { get; set; }
    public string UserId { get; set; }
    public bool IsAuthenticated { get; set; }
    public string UserRole { get; set; }
    public string ExceptionMessage { get; set; }
    public Exception Exception { get; set; }
    public IQueryCollection QueryCollection { get; set; }
}
