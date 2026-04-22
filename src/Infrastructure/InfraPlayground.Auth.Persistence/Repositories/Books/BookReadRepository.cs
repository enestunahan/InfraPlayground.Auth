using InfraPlayground.Auth.Application.Common.Repositories.Books;
using InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;
using InfraPlayground.Auth.Domain.Entities;
using InfraPlayground.Auth.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Persistence.Repositories.Books;

public sealed class BookReadRepository(InfraPlaygroundAuthDbContext context)
    : ReadRepository<Book>(context), IBookReadRepository
{
    public async Task<IReadOnlyList<HomePageBookDto>> GetHomePageBooksAsync(CancellationToken cancellationToken = default)
    {
        return await Table
            .AsNoTracking()
            .OrderBy(book => book.Title)
            .Select(book => new HomePageBookDto(
                book.Id,
                book.Title,
                book.Description,
                book.Isbn,
                book.PublicationYear,
                book.Price))
            .ToListAsync(cancellationToken);
    }
}
