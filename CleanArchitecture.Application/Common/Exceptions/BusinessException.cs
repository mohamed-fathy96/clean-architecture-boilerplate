namespace CleanArchitecture.Application.Common.Exceptions;

public abstract class BusinessException(string message) : Exception(message);
