using InfraPlayground.Auth.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Persistence.Contexts;

public sealed class InfraPlaygroundAuthDbContext(DbContextOptions<InfraPlaygroundAuthDbContext> options)
    : DbContext(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfraPlaygroundAuthDbContext).Assembly);
    }
}
