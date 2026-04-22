using InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;
using InfraPlayground.Auth.Domain.Entities;

namespace InfraPlayground.Auth.Application.Common.Repositories.Books;

public interface IBookReadRepository : IReadRepository<Book>
{
    Task<IReadOnlyList<HomePageBookDto>> GetHomePageBooksAsync(CancellationToken cancellationToken = default);
}
