namespace CleanArchitecture.Application.Common.Exceptions;

public class NotFoundException(string message) : BusinessException(message);

