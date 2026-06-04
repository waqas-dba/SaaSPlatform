using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/tenant/catalog/attribute-templates")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class TenantAttributeTemplatesController : TenantApiControllerBase
{
    private readonly IProductAttributeTemplateService _templateService;
    private readonly IProductAttributeGroupService _groupService;

    public TenantAttributeTemplatesController(
        IProductAttributeTemplateService templateService,
        IProductAttributeGroupService groupService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _templateService = templateService;
        _groupService = groupService;
    }

    [HttpGet("platform/{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetPlatformTemplates(string storeTypeCode, CancellationToken ct)
    {
        var templates = await _templateService.GetPlatformTemplatesAsync(storeTypeCode);
        return OkResponse(templates);
    }

    [HttpGet("{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetTenantTemplates(string storeTypeCode, CancellationToken ct)
    {
        var tenantId = RequireTenantId();
        var templates = await _templateService.GetTenantTemplatesAsync(tenantId, storeTypeCode);
        return OkResponse(templates);
    }

    [HttpPost("override")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> OverridePlatformTemplate(
        [FromBody] OverrideAttributeTemplateRequest request,
        CancellationToken ct)
    {
        request.TenantId = RequireTenantId();
        var template = await _templateService.OverridePlatformTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPost("custom")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> CreateCustomTemplate(
        [FromBody] CreateAttributeTemplateRequest request,
        CancellationToken ct)
    {
        request.TenantId = RequireTenantId();
        var template = await _templateService.CreateTenantTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{templateId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> UpdateTenantTemplate(
        Guid templateId,
        [FromBody] UpdateAttributeTemplateRequest request,
        CancellationToken ct)
    {
        await _templateService.UpdateTenantTemplateAsync(templateId, RequireTenantId(), request);
        return UpdatedResponse();
    }

    [HttpDelete("{templateId}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> DeleteTenantTemplate(Guid templateId, CancellationToken ct)
    {
        await _templateService.DeleteTenantTemplateAsync(templateId, RequireTenantId());
        return DeletedResponse();
    }

    [HttpPost("groups")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateAttributeGroupRequest request,
        CancellationToken ct)
    {
        request.TenantId = RequireTenantId();
        var group = await _groupService.CreateAsync(request);
        return CreatedResponse(group);
    }

    [HttpPost("assign")]
    [RequiresPermission(Permissions.Catalog.TemplatesAssign)]
    public async Task<IActionResult> AssignTemplate(
        [FromBody] AssignTemplateRequest request,
        CancellationToken ct)
    {
        request.TenantId = RequireTenantId();
        await _templateService.AssignTemplateAsync(request);
        return OkResponse("Template assigned successfully.");
    }

    [HttpDelete("assign")]
    [RequiresPermission(Permissions.Catalog.TemplatesAssign)]
    public async Task<IActionResult> UnassignTemplate(
        [FromBody] UnassignTemplateRequest request,
        CancellationToken ct)
    {
        await _templateService.UnassignTemplateAsync(RequireTenantId(), request.StoreId, request.TemplateId);
        return DeletedResponse();
    }

    [HttpGet("resolved/{storeId}/{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetResolved(
        Guid storeId,
        string storeTypeCode,
        CancellationToken ct)
    {
        var resolved = await _templateService.GetResolvedAttributesAsync(storeId, RequireTenantId(), storeTypeCode);
        return OkResponse(resolved);
    }

    [HttpPost("store-override")]
    [RequiresPermission(Permissions.Catalog.TemplatesToggleStore)]
    public async Task<IActionResult> SetStoreOverride(
        [FromBody] StoreAttributeOverrideRequest request,
        CancellationToken ct)
    {
        await _templateService.SetStoreAttributeOverrideAsync(request);
        return OkResponse("Store attribute override saved.");
    }
}

public record UnassignTemplateRequest(Guid? StoreId, Guid TemplateId);