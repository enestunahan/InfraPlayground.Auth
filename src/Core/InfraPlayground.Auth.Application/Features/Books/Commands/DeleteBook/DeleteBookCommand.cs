using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.DeleteBook;

public sealed record DeleteBookCommand(Guid Id) : IRequest<bool>;
