using System.Security.Claims;
using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Common.Security;
using InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomeController(ISender sender, ICurrentUserService currentUserService) : ControllerBase
{
    [HttpGet("public")]
    [AllowAnonymous]
    public IActionResult PublicInfo()
    {
        return Ok(new
        {
            message = "Bu endpoint herkese açık.",
            serverTimeUtc = DateTime.UtcNow
        });
    }

    [HttpGet("books")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Editor},{AppRoles.User},{AppRoles.Viewer}")]
    [ProducesResponseType(typeof(IReadOnlyList<HomePageBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HomePageBookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await sender.Send(new GetHomePageBooksQuery(), cancellationToken);
        return Ok(books);
    }

    [HttpGet("me")]
    [Authorize]
    public IActionResult Me()
    {
        return Ok(new
        {
            userId = currentUserService.UserId,
            userName = currentUserService.UserName,
            roles = User.FindAll(ClaimTypes.Role).Select(x => x.Value).ToArray()
        });
    }

    [HttpGet("admin-or-editor")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Editor}")]
    public IActionResult AdminOrEditor()
    {
        return Ok(new
        {
            message = "Bu endpoint sadece Admin veya Editor rollerine açık."
        });
    }

    [HttpGet("user-and-above")]
    [Authorize(Roles = $"{AppRoles.Admin},{AppRoles.Editor},{AppRoles.User}")]
    public IActionResult UserAndAbove()
    {
        return Ok(new
        {
            message = "Bu endpoint Admin, Editor ve User rollerine açık."
        });
    }

    [HttpGet("admin-only")]
    [Authorize(Roles = AppRoles.Admin)]
    public IActionResult AdminOnly()
    {
        return Ok(new
        {
            message = "Bu endpoint sadece Admin rolüne açık."
        });
    }

    // ============================================================
    //  POLICY ÖRNEKLERİ
    // ============================================================

    /// <summary>
    /// Policy örneği: User rolü VE 18+ yaş.
    /// İçeride iki requirement var (RolesRequirement + MinimumAgeRequirement),
    /// ikisinin de Succeed olması gerekir (AND).
    ///
    /// Test akışı:
    ///   - admin (30y, ama User rolü yok)        -> 403 (rol fail)
    ///   - enes.editor (25y, User rolü yok)      -> 403
    ///   - enes.user (17y, User rolü VAR)        -> 403 (yaş fail)
    ///   - enes.viewer (22y, User rolü yok)      -> 403
    ///   - 18+ yaşında User rolüne sahip biri    -> 200
    /// </summary>
    [HttpGet("adult-user")]
    [Authorize(Policy = Policies.AdultUser)]
    public IActionResult AdultUser()
    {
        return Ok(new
        {
            message = "Bu endpoint sadece User rolüne sahip ve 18 yaşından büyük kullanıcılara açık."
        });
    }

    /// <summary>
    /// Permission örneği: "Books.Read" izni gerekiyor.
    /// Hangi rol bu permission'a sahip onu RolePermissions belirler.
    /// Endpoint hangi rol olduğunu BİLMEZ — sadece permission ismini bilir.
    /// </summary>
    [HttpGet("books-permission-test")]
    [Authorize(Policy = Permissions.Books.Read)]
    public IActionResult BooksPermissionTest()
    {
        return Ok(new
        {
            message = "Bu endpoint 'Books.Read' permission'una sahip kullanıcılara açık.",
            currentUserPermissions = User.FindAll(Permissions.ClaimType).Select(x => x.Value).ToArray()
        });
    }
}
