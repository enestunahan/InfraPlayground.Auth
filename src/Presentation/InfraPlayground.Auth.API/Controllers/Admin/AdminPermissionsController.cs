using InfraPlayground.Auth.Application.Common.Authorization;
using InfraPlayground.Auth.Application.Features.Permissions.Commands.CreatePermission;
using InfraPlayground.Auth.Application.Features.Permissions.Commands.DeletePermission;
using InfraPlayground.Auth.Application.Features.Permissions.Commands.UpdatePermission;
using InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissionById;
using InfraPlayground.Auth.Application.Features.Permissions.Queries.GetPermissions;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace InfraPlayground.Auth.API.Controllers.Admin;

[ApiController]
[Route("api/admin/permissions")]
public sealed class AdminPermissionsController(ISender sender) : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = Permissions.PermissionManagement.Read)]
    [ProducesResponseType(typeof(GetPermissionsQueryResponse), StatusCodes.Status200OK)]
    public async Task<ActionResult<GetPermissionsQueryResponse>> GetPermissions(CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetPermissionsQuery(), cancellationToken);
        return Ok(response);
    }

    [HttpGet("{id:guid}")]
    [Authorize(Policy = Permissions.PermissionManagement.Read)]
    [ProducesResponseType(typeof(PermissionDetailDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PermissionDetailDto>> GetPermissionById(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(new GetPermissionByIdQuery(id), cancellationToken);
        if (response is null)
            return NotFound();

        return Ok(response);
    }

    [HttpPost]
    [Authorize(Policy = Permissions.PermissionManagement.Create)]
    [ProducesResponseType(typeof(CreatePermissionCommandResponse), StatusCodes.Status201Created)]
    public async Task<ActionResult<CreatePermissionCommandResponse>> CreatePermission(
        [FromBody] CreatePermissionCommand command,
        CancellationToken cancellationToken)
    {
        var response = await sender.Send(command, cancellationToken);
        return Created($"/api/admin/permissions/{response.Id}", response);
    }

    [HttpPut("{id:guid}")]
    [Authorize(Policy = Permissions.PermissionManagement.Update)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UpdatePermission(
        [FromRoute] Guid id,
        [FromBody] UpdatePermissionRequest request,
        CancellationToken cancellationToken)
    {
        await sender.Send(new UpdatePermissionCommand(id, request.Code, request.Description), cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize(Policy = Permissions.PermissionManagement.Delete)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeletePermission(
        [FromRoute] Guid id,
        CancellationToken cancellationToken)
    {
        await sender.Send(new DeletePermissionCommand(id), cancellationToken);
        return NoContent();
    }
}

public sealed record UpdatePermissionRequest(
    string Code,
    string Description);
