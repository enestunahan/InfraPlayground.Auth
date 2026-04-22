using InfraPlayground.Auth.Application.Common.Repositories.Books;
using InfraPlayground.Auth.Domain.Entities;
using InfraPlayground.Auth.Persistence.Contexts;

namespace InfraPlayground.Auth.Persistence.Repositories.Books;

public sealed class BookWriteRepository(InfraPlaygroundAuthDbContext context)
    : WriteRepository<Book>(context), IBookWriteRepository;
