using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.CreateBook;

public sealed record CreateBookCommand(
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    decimal Price) : IRequest<CreateBookCommandResponse>;

public sealed record CreateBookCommandResponse(Guid Id);
