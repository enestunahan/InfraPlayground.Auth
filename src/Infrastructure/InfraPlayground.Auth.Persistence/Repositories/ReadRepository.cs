using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Common;
using InfraPlayground.Auth.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace InfraPlayground.Auth.Persistence.Repositories;

public class ReadRepository<T>(InfraPlaygroundAuthDbContext context) : IReadRepository<T> where T : BaseEntity
{
    private readonly InfraPlaygroundAuthDbContext _context = context;

    public DbSet<T> Table => _context.Set<T>();

    public IQueryable<T> GetAll(bool tracking = true)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = query.AsNoTracking();

        return query;
    }

    public IQueryable<T> GetWhere(Expression<Func<T, bool>> method, bool tracking = true)
    {
        var query = Table.Where(method);
        if (!tracking)
            query = query.AsNoTracking();

        return query;
    }

    public async Task<T?> GetSingleAsync(
        Expression<Func<T, bool>> method,
        bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = Table.AsNoTracking();

        return await query.FirstOrDefaultAsync(method, cancellationToken);
    }

    public async Task<T?> GetByIdAsync(
        string id,
        bool tracking = true,
        CancellationToken cancellationToken = default)
    {
        var query = Table.AsQueryable();
        if (!tracking)
            query = Table.AsNoTracking();

        return await query.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id), cancellationToken);
    }
}
