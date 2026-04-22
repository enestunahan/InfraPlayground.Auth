using InfraPlayground.Auth.Application.Common.Repositories.Books;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed class GetBooksForAdminQueryHandler(IBookReadRepository bookReadRepository)
    : IRequestHandler<GetBooksForAdminQueryRequest, GetBooksForAdminQueryResponse>
{
    public async Task<GetBooksForAdminQueryResponse> Handle(
        GetBooksForAdminQueryRequest request,
        CancellationToken cancellationToken)
    {
        var books = await bookReadRepository.GetAll(false)
            .OrderBy(book => book.Title)
            .Select(book => new AdminBookListItemDto(
                book.Id,
                book.Title,
                book.Description,
                book.Isbn,
                book.PublicationYear,
                book.Price))
            .ToListAsync(cancellationToken);

        return new GetBooksForAdminQueryResponse
        {
            Books = books
        };
    }
}
