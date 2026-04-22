using InfraPlayground.Auth.Domain.Common;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Application.Common.Repositories;

public interface IRepository<T> where T : BaseEntity
{
    DbSet<T> Table { get; }
}
