namespace CleanArchitecture.Application.Common.Exceptions;

public class BadRequestException(string message) : BusinessException(message);

