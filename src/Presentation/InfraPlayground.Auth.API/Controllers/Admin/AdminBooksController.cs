using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Features.Books.Commands.CreateBook;
using InfraPlayground.Auth.Application.Features.Books.Commands.DeleteBook;
using InfraPlayground.Auth.Application.Features.Books.Commands.UpdateBook;
using InfraPlayground.Auth.Application.Features.Books.Queries.GetBooksForAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers.Admin;

[ApiController]
[Route("api/admin/books")]
[Authorize(Roles = AppRoles.Admin)]
public sealed class AdminBooksController(ISender sender) : ControllerBase
{
    [HttpGet]
    [ProducesResponseType(typeof(GetBooksForAdminQueryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetBooksForAdminQueryResponse>> GetBooksForAdmin(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetBooksForAdminQueryRequest(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [ProducesResponseType(typeof(CreateBookCommandResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBookCommandResponse>> CreateBook(
        [FromBody] CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Created($"/api/admin/books/{response.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdateBook(
        [FromRoute] Guid id,
        [FromBody] UpdateBookRequest request,
        CancellationToken cancellationToken)
    {
        var updated = await sender.Send(
            new UpdateBookCommand(id, request.Title, request.Description, request.Isbn, request.PublicationYear, request.Price),
            cancellationToken);

        if (!updated)
            return NotFound();

        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteBook([FromRoute] Guid id, CancellationToken cancellationToken)
    {
        var removed = await sender.Send(new DeleteBookCommand(id), cancellationToken);
        if (!removed)
            return NotFound();

        return NoContent();
    }
}

public sealed record UpdateBookRequest(
    string Title,
    string? Description,
    string Isbn,
    int PublicationYear,
    decimal Price);
