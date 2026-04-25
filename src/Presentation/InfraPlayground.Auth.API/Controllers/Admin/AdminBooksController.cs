using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Features.Books.Commands.CreateBook;
using InfraPlayground.Auth.Application.Features.Books.Commands.DeleteBook;
using InfraPlayground.Auth.Application.Features.Books.Commands.UpdateBook;
using InfraPlayground.Auth.Application.Features.Books.Queries.GetBooksForAdmin;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers.Admin;

// Controller seviyesinde authorize kullanmıyoruz çünkü her endpoint farklı
// permission gerektiriyor. Yine de "en azından authenticated olmalı" demek
// için boş [Authorize] de eklenebilir; biz burayı tamamen permission'lara
// devrediyoruz çünkü zaten her endpoint kendi izniyle korunuyor.
[ApiController]
[Route("api/admin/books")]
public sealed class AdminBooksController(ISender sender) : ControllerBase
{
    // PERMISSION-BASED:
    //   - Endpoint hangi rolün geleceğini bilmez, sadece "Books.Read" ister.
    //   - Admin, Editor, User, Viewer hepsinde Books.Read permission'ı var.
    //   - Yarın "Manager" diye yeni bir rol gelse, sadece RolePermissions'a
    //     ekleyince bu endpoint'e erişebilir. Endpoint'e dokunmaya gerek yok.
    [HttpGet]
    [Authorize(Policy = Permissions.Books.Read)]
    [ProducesResponseType(typeof(GetBooksForAdminQueryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetBooksForAdminQueryResponse>> GetBooksForAdmin(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetBooksForAdminQueryRequest(), cancellationToken);
        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.Books.Create)]  // Admin + Editor
    [ProducesResponseType(typeof(CreateBookCommandResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreateBookCommandResponse>> CreateBook(
        [FromBody] CreateBookCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Created($"/api/admin/books/{response.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.Books.Update)]  // Admin + Editor
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
    [Authorize(Policy = Permissions.Books.Delete)]  // Sadece Admin
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
