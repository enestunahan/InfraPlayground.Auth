using InfraPlayground.Auth.Application.Common.Repositories.Books;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.UpdateBook;

public sealed class UpdateBookCommandHandler(
    IBookReadRepository bookReadRepository,
    IBookWriteRepository bookWriteRepository)
    : IRequestHandler<UpdateBookCommand, bool>
{
    public async Task<bool> Handle(UpdateBookCommand request, CancellationToken cancellationToken)
    {
        var book = await bookReadRepository.GetByIdAsync(
            request.Id.ToString(),
            tracking: true,
            cancellationToken: cancellationToken);

        if (book is null)
            return false;

        var existingBookWithSameIsbn = await bookReadRepository.GetSingleAsync(
            x => x.Isbn == request.Isbn && x.Id != request.Id,
            tracking: false,
            cancellationToken: cancellationToken);

        if (existingBookWithSameIsbn is not null)
            throw new InvalidOperationException($"ISBN '{request.Isbn}' başka bir kayıtta kullanılıyor.");

        book.Title = request.Title;
        book.Description = request.Description;
        book.Isbn = request.Isbn;
        book.PublicationYear = request.PublicationYear;
        book.Price = request.Price;

        bookWriteRepository.Update(book);
        await bookWriteRepository.SaveAsync(cancellationToken);

        return true;
    }
}
