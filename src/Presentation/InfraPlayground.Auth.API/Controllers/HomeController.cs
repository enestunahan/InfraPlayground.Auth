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
}
