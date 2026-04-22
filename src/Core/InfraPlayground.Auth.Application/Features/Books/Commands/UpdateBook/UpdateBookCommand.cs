using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.UpdateBook;

public sealed record UpdateBookCommand(
    Guid Id,
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    decimal Price) : IRequest<bool>;
