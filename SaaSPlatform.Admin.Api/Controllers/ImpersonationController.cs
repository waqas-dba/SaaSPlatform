using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/impersonation")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class ImpersonationController : ControllerBase
{
    private readonly IImpersonationService _impersonationService;
    private readonly ICurrentUserService _currentUser;

    public ImpersonationController(
        IImpersonationService impersonationService,
        ICurrentUserService currentUser)
    {
        _impersonationService = impersonationService;
        _currentUser = currentUser;
    }

    [HttpPost("tenant/{tenantId}")]
    [RequiresPermission(Permissions.Platform.CreateImpersonation)]
    public async Task<IActionResult> ImpersonateTenant(
        Guid tenantId,
        [FromBody] ImpersonationRequest request,
        CancellationToken ct)
    {
        if (_currentUser.UserId == null)
            return Unauthorized();

        var (token, expiresAt) = await _impersonationService.CreateImpersonationTokenAsync(
            _currentUser.UserId.Value,
            tenantId,
            request.Reason);

        return Ok(new
        {
            accessToken = token,
            expiresAt,
            tenantId,
            message = $"Impersonation token created for tenant {tenantId}. " +
                      $"Use this token with x-tenant-id header for tenant-scoped operations. " +
                      $"Token expires at {expiresAt:O}."
        });
    }
}

public class ImpersonationRequest
{
    public string Reason { get; set; } = default!;
}