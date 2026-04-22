using InfraPlayground.Auth.Domain.Common;

namespace InfraPlayground.Auth.Application.Common.Repositories;

public interface IWriteRepository<T> : IRepository<T> where T : BaseEntity
{
    Task<bool> AddAsync(T model, CancellationToken cancellationToken = default);
    Task<bool> AddRangeAsync(List<T> datas, CancellationToken cancellationToken = default);
    bool Remove(T model);
    bool RemoveRange(List<T> datas);
    Task<bool> RemoveAsync(string id, CancellationToken cancellationToken = default);
    bool Update(T model);
    Task<int> SaveAsync(CancellationToken cancellationToken = default);
}
