namespace CleanArchitecture.Application.Common.Exceptions;

public class UnauthorizedBusinessException(string message = "You are not allowed to access this resource")
    : BusinessException(message);
