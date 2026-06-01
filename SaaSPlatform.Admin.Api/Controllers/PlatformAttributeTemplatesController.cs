// SaaSPlatform.Admin.Api/Controllers/PlatformAttributeTemplatesController.cs
using CoreKit.Catalog.Constants;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

/// <summary>
/// Platform admin manages the global base attribute templates.
/// These are visible to all tenants as read-only defaults.
/// </summary>
[ApiController]
[Route("api/admin/catalog/attribute-templates")]
[Authorize]
public class PlatformAttributeTemplatesController : ApiControllerBase
{
    private readonly IProductAttributeTemplateService _templateService;
    private readonly IProductAttributeGroupService _groupService;

    public PlatformAttributeTemplatesController(
        IProductAttributeTemplateService templateService,
        IProductAttributeGroupService groupService)
    {
        _templateService = templateService;
        _groupService = groupService;
    }

    // ── Groups ────────────────────────────────────────────────────────────

    [HttpGet("groups/{storeTypeCode}")]
    [RequiresPermission(CatalogPermissions.TemplatesView)]
    public async Task<IActionResult> GetGroups(string storeTypeCode)
    {
        var groups = await _groupService.GetByStoreTypeAsync(storeTypeCode);
        return OkResponse(groups);
    }

    [HttpPost("groups")]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateAttributeGroupRequest request)
    {
        // Platform-only endpoint — force TenantId to null.
        request = request with { TenantId = null };
        var group = await _groupService.CreateAsync(request);
        return CreatedResponse(group);
    }

    [HttpPut("groups/{groupId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> UpdateGroup(
        Guid groupId,
        [FromBody] UpdateGroupRequest request)
    {
        await _groupService.UpdateAsync(groupId, request.Name, request.SortOrder);
        return UpdatedResponse();
    }

    [HttpDelete("groups/{groupId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> DeleteGroup(Guid groupId)
    {
        await _groupService.DeleteAsync(groupId);
        return DeletedResponse();
    }

    // ── Templates ─────────────────────────────────────────────────────────

    [HttpGet("{storeTypeCode}")]
    [RequiresPermission(CatalogPermissions.TemplatesView)]
    public async Task<IActionResult> GetByStoreType(string storeTypeCode)
    {
        var templates =
            await _templateService.GetPlatformTemplatesAsync(storeTypeCode);
        return OkResponse(templates);
    }

    [HttpPost]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAttributeTemplateRequest request)
    {
        var template =
            await _templateService.CreatePlatformTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{templateId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> Update(
        Guid templateId,
        [FromBody] UpdateAttributeTemplateRequest request)
    {
        await _templateService.UpdatePlatformTemplateAsync(templateId, request);
        return UpdatedResponse();
    }

    [HttpDelete("{templateId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManagePlatform)]
    public async Task<IActionResult> Delete(Guid templateId)
    {
        await _templateService.DeletePlatformTemplateAsync(templateId);
        return DeletedResponse();
    }
}

public record UpdateGroupRequest(string Name, int SortOrder);