using InfraPlayground.Auth.Domain.Common;

namespace InfraPlayground.Auth.Domain.Entities;

public sealed class Book : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public int PublicationYear { get; set; }
    public decimal Price { get; set; }
}
