// SaaSPlatform.Tenant.Api/Controllers/AddonGroupsController.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/addon-groups")]
[Authorize]
public class AddonGroupsController : ApiControllerBase
{
    private readonly IAddonService _addonService;
    private readonly ITenantContext _tenantContext;

    public AddonGroupsController(
        IAddonService addonService,
        ITenantContext tenantContext)
    {
        _addonService = addonService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _addonService
            .GetGroupsByTenantAsync(RequireTenantId());

        return OkResponse(groups);
    }

    [HttpGet("{groupId:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid groupId)
    {
        var group = await _addonService
            .GetGroupByIdAsync(groupId, RequireTenantId());

        return group is null ? NotFound() : OkResponse(group);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.create")]
    public async Task<IActionResult> Create(
        CreateAddonGroupRequest request)
    {
        var group = await _addonService
            .CreateGroupAsync(RequireTenantId(), request);

        return CreatedResponse(group);
    }

    [HttpPut("{groupId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Update(
        Guid groupId,
        UpdateAddonGroupRequest request)
    {
        var group = await _addonService
            .UpdateGroupAsync(groupId, RequireTenantId(), request);

        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}")]
    [RequiresPermission("catalog.products.delete")]
    public async Task<IActionResult> Delete(Guid groupId)
    {
        await _addonService.DeleteGroupAsync(groupId, RequireTenantId());
        return DeletedResponse();
    }

    // ── Addons within a group ────────────────────────────────────────

    [HttpPost("{groupId:guid}/addons")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> AddAddon(
        Guid groupId,
        AddAddonToGroupRequest request)
    {
        var addon = await _addonService
            .AddToGroupAsync(groupId, RequireTenantId(), request);

        return CreatedResponse(addon);
    }

    [HttpPut("{groupId:guid}/addons/{addonId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> UpdateAddon(
        Guid groupId,
        Guid addonId,
        AddAddonToGroupRequest request)
    {
        var addon = await _addonService
            .UpdateAddonAsync(addonId, RequireTenantId(), request);

        return OkResponse(addon);
    }

    [HttpDelete("{groupId:guid}/addons/{addonId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> RemoveAddon(
        Guid groupId,
        Guid addonId)
    {
        await _addonService
            .RemoveFromGroupAsync(addonId, RequireTenantId());

        return DeletedResponse();
    }

    private Guid RequireTenantId() =>
        _tenantContext.TenantId
        ?? throw new UnauthorizedAccessException(
            "Tenant context is missing.");
}