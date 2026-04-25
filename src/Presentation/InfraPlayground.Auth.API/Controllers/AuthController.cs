using System.Security.Claims;
using InfraPlayground.Auth.Application.Common.Security;
using InfraPlayground.Auth.Application.Features.Auth.Commands.Login;
using InfraPlayground.Auth.Application.Features.Auth.Commands.Logout;
using InfraPlayground.Auth.Application.Features.Auth.Commands.RefreshTokenLogin;
using InfraPlayground.Auth.Application.Features.Auth.Commands.Register;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers;

[ApiController]
[Route("api/auth")]
public sealed class AuthController(ISender sender) : ControllerBase
{
    [HttpPost("register")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<RegisterResponse>> Register(
        [FromBody] RegisterCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Ok(response);
    }

    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenDto>> Login(
        [FromBody] LoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(command, cancellationToken);
        return Ok(token);
    }

    [HttpPost("refresh-token-login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<TokenDto>> RefreshTokenLogin(
        [FromBody] RefreshTokenLoginCommand command,
        CancellationToken cancellationToken)
    {
        var token = await sender.Send(command, cancellationToken);
        return Ok(token);
    }

    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
                     ?? throw new UnauthorizedAccessException();

        await sender.Send(new LogoutCommand(userId), cancellationToken);
        return NoContent();
    }
}
