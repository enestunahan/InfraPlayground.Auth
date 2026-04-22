using InfraPlayground.Auth.Application.Common.Repositories;
using InfraPlayground.Auth.Domain.Common;
using InfraPlayground.Auth.Persistence.Contexts;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;

namespace InfraPlayground.Auth.Persistence.Repositories;

public class WriteRepository<T>(InfraPlaygroundAuthDbContext context) : IWriteRepository<T> where T : BaseEntity
{
    private readonly InfraPlaygroundAuthDbContext _context = context;

    public DbSet<T> Table => _context.Set<T>();

    public async Task<bool> AddAsync(T model, CancellationToken cancellationToken = default)
    {
        EntityEntry<T> entityEntry = await Table.AddAsync(model, cancellationToken);
        return entityEntry.State == EntityState.Added;
    }

    public async Task<bool> AddRangeAsync(List<T> datas, CancellationToken cancellationToken = default)
    {
        await Table.AddRangeAsync(datas, cancellationToken);
        return true;
    }

    public bool Remove(T model)
    {
        EntityEntry<T> entityEntry = Table.Remove(model);
        return entityEntry.State == EntityState.Deleted;
    }

    public bool RemoveRange(List<T> datas)
    {
        Table.RemoveRange(datas);
        return true;
    }

    public async Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default)
    {
        var model = await Table.FirstOrDefaultAsync(data => data.Id == Guid.Parse(id), cancellationToken);
        if (model is null)
            return false;

        return Remove(model);
    }

    public bool Update(T model)
    {
        EntityEntry entityEntry = Table.Update(model);
        return entityEntry.State == EntityState.Modified;
    }

    public async Task<int> SaveAsync(CancellationToken cancellationToken = default)
        => await _context.SaveChangesAsync(cancellationToken);
}
