using InfraPlayground.Auth.Application.Common.Repositories.Books;
using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Commands.DeleteBook;

public sealed class DeleteBookCommandHandler(IBookWriteRepository bookWriteRepository)
    : IRequestHandler<DeleteBookCommand, bool>
{
    public async Task<bool> Handle(DeleteBookCommand request, CancellationToken cancellationToken)
    {
        var removed = await bookWriteRepository.RemoveAsync(request.Id.ToString(), cancellationToken);
        if (!removed)
            return false;

        await bookWriteRepository.SaveAsync(cancellationToken);
        return true;
    }
}
