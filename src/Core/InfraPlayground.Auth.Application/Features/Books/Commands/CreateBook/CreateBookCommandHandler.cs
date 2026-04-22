using InfraPlayground.Auth.Application.Common.Repositories.Books;
using InfraPlayground.Auth.Domain.Entities;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.CreateBook;

public sealed class CreateBookCommandHandler(
    IBookReadRepository bookReadRepository,
    IBookWriteRepository bookWriteRepository)
    : IRequestHandler<CreateBookCommand, CreateBookCommandResponse>
{
    public async Task<CreateBookCommandResponse> Handle(
        CreateBookCommand request,
        CancellationToken cancellationToken)
    {
        var existingBook = await bookReadRepository.GetSingleAsync(
            book => book.Isbn == request.Isbn,
            tracking: false,
            cancellationToken: cancellationToken);

        if (existingBook is not null)
            throw new InvalidOperationException($"ISBN '{request.Isbn}' zaten kayıtlı.");

        var book = new Book
        {
            Id = Guid.NewGuid(),
            Title = request.Title,
            Description = request.Description,
            Isbn = request.Isbn,
            PublicationYear = request.PublicationYear,
            Price = request.Price
        };

        await bookWriteRepository.AddAsync(book, cancellationToken);
        await bookWriteRepository.SaveAsync(cancellationToken);

        return new CreateBookCommandResponse(book.Id);
    }
}
