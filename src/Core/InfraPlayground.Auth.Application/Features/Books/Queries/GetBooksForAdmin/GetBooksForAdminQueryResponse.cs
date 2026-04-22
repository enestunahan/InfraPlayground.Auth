namespace InfraPlayground.Auth.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed record AdminBookListItemDto(
    Guid Id,
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    decimal Price);

public sealed class GetBooksForAdminQueryResponse
{
    public IReadOnlyList<AdminBookListItemDto> Books { get; init; } = [];
}
