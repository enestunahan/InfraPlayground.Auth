namespace InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;

public sealed record HomePageBookDto(
    Guid Id,
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    decimal Price);
