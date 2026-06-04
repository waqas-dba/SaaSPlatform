using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v1/admin/roles/{roleId:guid}/document-requirements")]
[Authorize]
public class RoleDocumentRequirementsController : ApiControllerBase
{
    private readonly IRoleDocumentRequirementService _requirementService;

    public RoleDocumentRequirementsController(IRoleDocumentRequirementService requirementService)
        => _requirementService = requirementService;

    [HttpGet]
    [RequiresPermission(Permissions.Documents.View)]
    public async Task<IActionResult> GetAll(Guid roleId, CancellationToken ct)
    {
        var types = await _requirementService.GetRequiredDocumentsAsync(roleId);
        return OkResponse(types);
    }

    [HttpPost("{documentType}")]
    [RequiresPermission(Permissions.Documents.Upload)]
    public async Task<IActionResult> Add(Guid roleId, string documentType, CancellationToken ct)
    {
        await _requirementService.AddRequirementAsync(roleId, documentType);
        return CreatedResponse($"Document requirement '{documentType}' added.");
    }

    [HttpDelete("{documentType}")]
    [RequiresPermission(Permissions.Documents.Delete)]
    public async Task<IActionResult> Remove(Guid roleId, string documentType, CancellationToken ct)
    {
        await _requirementService.RemoveRequirementAsync(roleId, documentType);
        return DeletedResponse();
    }
}