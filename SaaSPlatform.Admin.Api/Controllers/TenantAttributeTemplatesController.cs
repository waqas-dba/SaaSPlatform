// SaaSPlatform.Tenant.Api/Controllers/TenantAttributeTemplatesController.cs
using CoreKit.Catalog.Constants;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Interfaces;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

/// <summary>
/// Tenant admin manages their scoped attribute customisations:
///   - Override platform templates (tenant-scoped copy, original untouched).
///   - Add custom attributes not present in the base template.
///   - Assign templates to specific stores or tenant-wide.
///   - View the resolved (effective) attribute list for a store.
/// </summary>
[ApiController]
[Route("api/tenant/catalog/attribute-templates")]
[Authorize]
public class TenantAttributeTemplatesController : ApiControllerBase
{
    private readonly IProductAttributeTemplateService _templateService;
    private readonly IProductAttributeGroupService _groupService;
    private readonly ICurrentUserService _currentUser;
    private readonly ITenantContext _tenantContext;

    public TenantAttributeTemplatesController(
        IProductAttributeTemplateService templateService,
        IProductAttributeGroupService groupService,
        ICurrentUserService currentUser,
        ITenantContext tenantContext)
    {
        _templateService = templateService;
        _groupService = groupService;
        _currentUser = currentUser;
        _tenantContext = tenantContext;
    }

    // ── View platform base templates ──────────────────────────────────────

    [HttpGet("platform/{storeTypeCode}")]
    [RequiresPermission(CatalogPermissions.TemplatesView)]
    public async Task<IActionResult> GetPlatformTemplates(string storeTypeCode)
    {
        var templates =
            await _templateService.GetPlatformTemplatesAsync(storeTypeCode);
        return OkResponse(templates);
    }

    // ── Tenant-scoped templates ───────────────────────────────────────────

    [HttpGet("{storeTypeCode}")]
    [RequiresPermission(CatalogPermissions.TemplatesView)]
    public async Task<IActionResult> GetTenantTemplates(string storeTypeCode)
    {
        var tenantId = RequireTenantId();
        var templates = await _templateService.GetTenantTemplatesAsync(
            tenantId, storeTypeCode);
        return OkResponse(templates);
    }

    /// <summary>
    /// Creates a tenant-scoped override of an existing platform template.
    /// Only the fields included in the request body are changed;
    /// the rest are copied from the platform template.
    /// </summary>
    [HttpPost("override")]
    [RequiresPermission(CatalogPermissions.TemplatesManageTenant)]
    public async Task<IActionResult> OverridePlatformTemplate(
        [FromBody] OverrideAttributeTemplateRequest request)
    {
        request = request with { TenantId = RequireTenantId() };
        var template =
            await _templateService.OverridePlatformTemplateAsync(request);
        return CreatedResponse(template);
    }

    /// <summary>
    /// Creates a brand-new custom attribute that has no platform equivalent.
    /// </summary>
    [HttpPost("custom")]
    [RequiresPermission(CatalogPermissions.TemplatesManageTenant)]
    public async Task<IActionResult> CreateCustomTemplate(
        [FromBody] CreateAttributeTemplateRequest request)
    {
        request = request with { TenantId = RequireTenantId() };
        var template =
            await _templateService.CreateTenantTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{templateId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManageTenant)]
    public async Task<IActionResult> UpdateTenantTemplate(
        Guid templateId,
        [FromBody] UpdateAttributeTemplateRequest request)
    {
        await _templateService.UpdateTenantTemplateAsync(
            templateId, RequireTenantId(), request);
        return UpdatedResponse();
    }

    [HttpDelete("{templateId}")]
    [RequiresPermission(CatalogPermissions.TemplatesManageTenant)]
    public async Task<IActionResult> DeleteTenantTemplate(Guid templateId)
    {
        await _templateService.DeleteTenantTemplateAsync(
            templateId, RequireTenantId());
        return DeletedResponse();
    }

    // ── Groups ────────────────────────────────────────────────────────────

    [HttpPost("groups")]
    [RequiresPermission(CatalogPermissions.TemplatesManageTenant)]
    public async Task<IActionResult> CreateGroup(
        [FromBody] CreateAttributeGroupRequest request)
    {
        request = request with { TenantId = RequireTenantId() };
        var group = await _groupService.CreateAsync(request);
        return CreatedResponse(group);
    }

    // ── Assignment ────────────────────────────────────────────────────────

    /// <summary>
    /// Assigns a template to a store (StoreId set) or to all stores
    /// of this tenant for the given store type (StoreId = null).
    /// </summary>
    [HttpPost("assign")]
    [RequiresPermission(CatalogPermissions.TemplatesAssign)]
    public async Task<IActionResult> AssignTemplate(
        [FromBody] AssignTemplateRequest request)
    {
        request = request with { TenantId = RequireTenantId() };
        await _templateService.AssignTemplateAsync(request);
        return OkResponse("Template assigned successfully.");
    }

    [HttpDelete("assign")]
    [RequiresPermission(CatalogPermissions.TemplatesAssign)]
    public async Task<IActionResult> UnassignTemplate(
        [FromBody] UnassignTemplateRequest request)
    {
        await _templateService.UnassignTemplateAsync(
            RequireTenantId(), request.StoreId, request.TemplateId);
        return DeletedResponse();
    }

    // ── Resolved view ─────────────────────────────────────────────────────

    /// <summary>
    /// Returns the fully-resolved attribute list for a store,
    /// with all three precedence layers applied.
    /// Use this to render the product creation form.
    /// </summary>
    [HttpGet("resolved/{storeId}/{storeTypeCode}")]
    [RequiresPermission(CatalogPermissions.TemplatesView)]
    public async Task<IActionResult> GetResolved(
        Guid storeId,
        string storeTypeCode)
    {
        var resolved = await _templateService.GetResolvedAttributesAsync(
            storeId, RequireTenantId(), storeTypeCode);
        return OkResponse(resolved);
    }

    // ── Store-level toggle ────────────────────────────────────────────────

    /// <summary>
    /// Store manager toggles IsRequired / IsVisible for an attribute
    /// on their specific store.
    /// </summary>
    [HttpPost("store-override")]
    [RequiresPermission(CatalogPermissions.TemplatesToggleStore)]
    public async Task<IActionResult> SetStoreOverride(
        [FromBody] StoreAttributeOverrideRequest request)
    {
        await _templateService.SetStoreAttributeOverrideAsync(request);
        return OkResponse("Store attribute override saved.");
    }

    // ── Helper ────────────────────────────────────────────────────────────

    private Guid RequireTenantId() =>
        _tenantContext.TenantId
        ?? throw new UnauthorizedAccessException("Tenant context is missing.");
}

public record UnassignTemplateRequest(Guid? StoreId, Guid TemplateId);