using System.Security.Claims;
using KnowledgeBase.Auth;
using KnowledgeBase.Tenant.Application;
using KnowledgeBase.Tenant.Domain;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace KnowledgeBase.Tenant.Api.Controllers;

[ApiController]
[Route("api/tenants")]
[Authorize]
public sealed class TenantsController : ControllerBase
{
    private readonly TenantAppService tenantService;

    public TenantsController(TenantAppService tenantService)
    {
        this.tenantService = tenantService;
    }

    [HttpPost]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<TenantDto>> Create(
        [FromBody] CreateTenantRequest request,
        CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User identity not found.");

        var tenant = await tenantService.CreateAsync(request.Name, userId, cancellationToken);
        return CreatedAtAction(nameof(Get), new { id = tenant.Id }, tenant);
    }

    [HttpGet]
    [ProducesResponseType(typeof(IReadOnlyList<TenantDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TenantDto>>> List(CancellationToken cancellationToken)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new InvalidOperationException("User identity not found.");

        var tenants = await tenantService.ListByUserAsync(userId, cancellationToken);
        return Ok(tenants);
    }

    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(TenantDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TenantDto>> Get(Guid id, CancellationToken cancellationToken)
    {
        var tenant = await tenantService.GetAsync(id, cancellationToken);
        return tenant is null ? NotFound() : Ok(tenant);
    }

    [HttpPost("{tenantId:guid}/members")]
    [Authorize(Policy = AuthPolicies.TenantAdministration)]
    [ProducesResponseType(typeof(MembershipDto), StatusCodes.Status201Created)]
    public async Task<ActionResult<MembershipDto>> AddMember(
        Guid tenantId,
        [FromBody] AddMemberRequest request,
        CancellationToken cancellationToken)
    {
        var membership = await tenantService.AddMemberAsync(
            tenantId, request.UserId, request.Role, cancellationToken);

        return Created(string.Empty, membership);
    }

    [HttpDelete("{tenantId:guid}/members/{userId}")]
    [Authorize(Policy = AuthPolicies.TenantAdministration)]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public async Task<IActionResult> RemoveMember(
        Guid tenantId,
        string userId,
        CancellationToken cancellationToken)
    {
        await tenantService.RemoveMemberAsync(tenantId, userId, cancellationToken);
        return NoContent();
    }
}

public sealed record CreateTenantRequest(string Name);

public sealed record AddMemberRequest(string UserId, MemberRole Role);
