using InfraPlayground.Auth.Domain.Entities;
using InfraPlayground.Auth.Persistence.Seed;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace InfraPlayground.Auth.Persistence.Configurations;

public sealed class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.ToTable("Books");

        builder.HasKey(book => book.Id);

        builder.Property(book => book.Title)
            .HasMaxLength(200)
            .IsRequired();

        builder.Property(book => book.Description)
            .HasMaxLength(1000);

        builder.Property(book => book.Isbn)
            .HasMaxLength(13)
            .IsRequired();

        builder.HasIndex(book => book.Isbn)
            .IsUnique();

        builder.Property(book => book.PublicationYear)
            .IsRequired();

        builder.Property(book => book.Price)
            .HasColumnType("numeric(10,2)")
            .IsRequired();

        builder.HasData(SeedData.Books);
    }
}
