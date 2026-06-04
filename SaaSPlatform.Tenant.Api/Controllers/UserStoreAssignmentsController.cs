using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/users/{userId:guid}/stores")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class UserStoreAssignmentsController : TenantApiControllerBase
{
    private readonly IUserStoreAssignmentService _assignmentService;
    private readonly IUserManagementService _userService;

    public UserStoreAssignmentsController(
        IUserStoreAssignmentService assignmentService,
        IUserManagementService userService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _assignmentService = assignmentService;
        _userService = userService;
    }

    [HttpGet]
    [RequiresPermission(Permissions.Users.View)]
    public async Task<IActionResult> GetAssignments(Guid userId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var user = await _userService.GetUserByIdAsync(userId, tenantId);
        if (user == null)
            return NotFound(new { success = false, errorCode = "NOT_FOUND", message = "User not found." });

        var storeIds = await _assignmentService.GetStoresForUserAsync(userId, tenantId, ct);
        return OkResponse(storeIds.Select(id => new { StoreId = id }));
    }

    [HttpPost]
    [RequiresPermission(Permissions.Users.AssignRole)]
    public async Task<IActionResult> Assign(Guid userId, [FromBody] AssignUserToStoreRequest request, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var user = await _userService.GetUserByIdAsync(userId, tenantId);
        if (user == null)
            return NotFound(new { success = false, errorCode = "NOT_FOUND", message = "User not found." });

        await _assignmentService.AssignUserToStoreAsync(userId, request.StoreId, tenantId, ct);
        return OkResponse($"User assigned to store {request.StoreId}.");
    }

    [HttpDelete("{storeId:guid}")]
    [RequiresPermission(Permissions.Users.RemoveRole)]
    public async Task<IActionResult> Unassign(Guid userId, Guid storeId, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        await _assignmentService.UnassignUserFromStoreAsync(userId, storeId, tenantId, ct);
        return DeletedResponse("User removed from store.");
    }
}

public class AssignUserToStoreRequest
{
    public Guid StoreId { get; set; }
}