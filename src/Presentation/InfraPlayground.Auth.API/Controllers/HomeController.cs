using InfraPlayground.Auth.Application.Features.Home.Queries.GetHomePageBooks;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers;

[ApiController]
[Route("api/home")]
public sealed class HomeController(ISender sender) : ControllerBase
{
    [HttpGet("public")]
    public IActionResult PublicInfo()
    {
        return Ok(new
        {
            message = "Bu endpoint herkese açık.",
            serverTimeUtc = DateTime.UtcNow
        });
    }

    [HttpGet("books")]
    [ProducesResponseType(typeof(IReadOnlyList<HomePageBookDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<HomePageBookDto>>> GetBooks(CancellationToken cancellationToken)
    {
        var books = await sender.Send(new GetHomePageBooksQuery(), cancellationToken);
        return Ok(books);
    }
}
