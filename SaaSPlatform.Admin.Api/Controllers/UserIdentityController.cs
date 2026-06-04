using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/admin/users/{userId:guid}/identity")]
[Authorize]
public class UserIdentityController : ApiControllerBase
{
    private readonly IUserIdentityService _identityService;

    public UserIdentityController(IUserIdentityService identityService)
        => _identityService = identityService;

    [HttpGet]
    [RequiresPermission(Permissions.Identity.View)]
    public async Task<IActionResult> Get(Guid userId, CancellationToken ct)
    {
        var cnic = await _identityService.GetCnicAsync(userId);
        return cnic is null ? NotFound() : OkResponse(new { cnic });
    }

    [HttpPost]
    [RequiresPermission(Permissions.Identity.Manage)]
    public async Task<IActionResult> Set(Guid userId, [FromBody] SetCnicRequest request, CancellationToken ct)
    {
        await _identityService.SetCnicAsync(userId, request.Cnic);
        return CreatedResponse("CNIC saved successfully.");
    }

    [HttpDelete]
    [RequiresPermission(Permissions.Identity.Manage)]
    public async Task<IActionResult> Delete(Guid userId, CancellationToken ct)
    {
        await _identityService.DeleteAsync(userId);
        return DeletedResponse();
    }
}

public class SetCnicRequest
{
    public string Cnic { get; set; } = default!;
}