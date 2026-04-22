namespace InfraPlayground.Auth.Application.Common.Exceptions;

public sealed class AuthenticationFailedException : Exception
{
    public AuthenticationFailedException(string message) : base(message)
    {
    }
}
