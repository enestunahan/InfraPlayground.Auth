using InfraPlayground.Auth.Domain.Entities;
using InfraPlayground.Auth.Domain.Entities.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace InfraPlayground.Auth.Persistence.Contexts;

public sealed class InfraPlaygroundAuthDbContext(DbContextOptions<InfraPlaygroundAuthDbContext> options)
    : IdentityDbContext<AppUser, AppRole, string>(options)
{
    public DbSet<Book> Books => Set<Book>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(InfraPlaygroundAuthDbContext).Assembly);
    }
}
