namespace InfraPlayground.Auth.Application.Common.Exceptions;

public class BusinessException : Exception
{
    public BusinessException(string message) : base(message)
    {
    }
}

public sealed class NotFoundException : BusinessException
{
    public NotFoundException(string message) : base(message)
    {
    }
}
