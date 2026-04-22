using InfraPlayground.Auth.Domain.Entities;

namespace InfraPlayground.Auth.Persistence.Seed;

internal static class SeedData
{
    public static readonly Book[] Books =
    [
        new()
        {
            Id = Guid.Parse("d6a6fd4f-8a61-47b2-87e4-c7d8f934c1d1"),
            Title = "Clean Code",
            Description = "Yazılım geliştirmede okunabilirlik ve sürdürülebilirlik prensipleri.",
            Isbn = "9780132350884",
            PublicationYear = 2008,
            Price = 42.50m
        },
        new()
        {
            Id = Guid.Parse("22fdfd36-83fd-45b6-aa09-c19f453f4fb2"),
            Title = "Domain-Driven Design",
            Description = "Karmaşık domain problemleri için modelleme yaklaşımı.",
            Isbn = "9780321125217",
            PublicationYear = 2003,
            Price = 55.00m
        },
        new()
        {
            Id = Guid.Parse("bb3dcb8d-f4c0-4bb2-87a4-b3db9fd11b7f"),
            Title = "The Pragmatic Programmer",
            Description = "Pratik yazılım geliştirme alışkanlıkları ve mühendislik bakışı.",
            Isbn = "9780201616224",
            PublicationYear = 1999,
            Price = 39.90m
        }
    ];
}
