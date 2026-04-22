using MediatR;

namespace InfraPlayground.Auth.Application.Features.Books.Queries.GetBooksForAdmin;

public sealed record GetBooksForAdminQueryRequest : IRequest<GetBooksForAdminQueryResponse>;
