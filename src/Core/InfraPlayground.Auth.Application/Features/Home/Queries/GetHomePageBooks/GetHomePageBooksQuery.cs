using MediatR;

namespace InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;

public sealed record GetHomePageBooksQuery : IRequest<IReadOnlyList<HomePageBookDto>>;
