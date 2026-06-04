using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/admin/catalog/attribute-templates")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
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

    // --- Group endpoints ---

    [HttpGet("groups/{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetGroups(string storeTypeCode, CancellationToken ct)
    {
        var groups = await _groupService.GetByStoreTypeAsync(storeTypeCode);
        return OkResponse(groups);
    }

    [HttpPost("groups")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateAttributeGroupRequest request,
        CancellationToken ct)
    {
        request.TenantId = null;   // platform group
        var group = await _groupService.CreateAsync(request);
        return CreatedResponse(group);
    }

    [HttpPut("groups/{groupId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> UpdateGroup(
        Guid groupId,
        [FromBody] UpdateGroupRequest request,
        CancellationToken ct)
    {
        await _groupService.UpdateAsync(groupId, request.Name, request.SortOrder);
        return UpdatedResponse();
    }

    [HttpDelete("groups/{groupId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> DeleteGroup(Guid groupId, CancellationToken ct)
    {
        await _groupService.DeleteAsync(groupId);
        return DeletedResponse();
    }

    // --- Template endpoints ---

    [HttpGet("{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetByStoreType(string storeTypeCode, CancellationToken ct)
    {
        var templates = await _templateService.GetPlatformTemplatesAsync(storeTypeCode);
        return OkResponse(templates);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Create(
        [FromBody] CreateAttributeTemplateRequest request,
        CancellationToken ct)
    {
        var template = await _templateService.CreatePlatformTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{templateId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Update(
        Guid templateId,
        [FromBody] UpdateAttributeTemplateRequest request,
        CancellationToken ct)
    {
        await _templateService.UpdatePlatformTemplateAsync(templateId, request);
        return UpdatedResponse();
    }

    [HttpDelete("{templateId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Delete(Guid templateId, CancellationToken ct)
    {
        await _templateService.DeletePlatformTemplateAsync(templateId);
        return DeletedResponse();
    }
}

public record UpdateGroupRequest(string Name, int SortOrder);