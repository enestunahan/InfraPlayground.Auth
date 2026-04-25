namespace InfraPlayground.Auth.Application.Common.Authorization;

/// <summary>
/// Policy isimlerini tek bir yerde tutuyoruz.
/// Sebebi: hem [Authorize(Policy = "...")] attribute'unda, hem de DI tarafında
/// AddPolicy("...") kaydında aynı string'e ihtiyacımız var. String literal yazmak
/// yerine sabitten okumak typo'ları engeller ve değişiklik tek noktadan yapılır.
/// </summary>
public static class Policies
{
    /// <summary>
    /// User rolüne sahip VE 18 yaşından büyük kullanıcılar.
    /// Hem rol hem custom requirement (yaş) içerdiği için klasik
    /// [Authorize(Roles = ...)] attribute'u ile yazılamaz — policy gerekiyor.
    /// </summary>
    public const string AdultUser = "AdultUser";
}
