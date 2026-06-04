using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/permissions")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class PermissionsController : ApiControllerBase
{
    private readonly IRoleRepository _roleRepo;

    public PermissionsController(IRoleRepository roleRepo) => _roleRepo = roleRepo;

    [HttpGet]
    [RequiresPermission(Permissions.PermissionsManagement.View)]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var perms = await _roleRepo.GetAllPermissionsAsync(ct);
        return OkResponse(perms.Select(p => new { p.Id, p.Name, p.PermissionModuleId }));
    }
}