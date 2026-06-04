using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/addon-groups")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class AddonGroupsController : TenantApiControllerBase
{
    private readonly IAddonService _addonService;

    public AddonGroupsController(IAddonService addonService, ITenantContext tenantContext)
        : base(tenantContext)
    {
        _addonService = addonService;
    }

    [HttpGet]
    [RequiresPermission("catalog.products.view")]

    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var groups = await _addonService.GetGroupsByTenantAsync(RequireTenantId(), ct);
        return OkResponse(groups);
    }

    [HttpGet("{groupId:guid}")]
    [RequiresPermission("catalog.products.view")]
   
    public async Task<IActionResult> GetById(Guid groupId, CancellationToken ct)
    {
        var group = await _addonService.GetGroupByIdAsync(groupId, RequireTenantId(), ct);
        return group is null ? NotFound() : OkResponse(group);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.create")]
   
    public async Task<IActionResult> Create(CreateAddonGroupRequest request, CancellationToken ct)
    {
        var group = await _addonService.CreateGroupAsync(RequireTenantId(), request, ct);
        return CreatedResponse(group);
    }

    [HttpPut("{groupId:guid}")]
    [RequiresPermission("catalog.products.update")]
   
    public async Task<IActionResult> Update(Guid groupId, UpdateAddonGroupRequest request, CancellationToken ct)
    {
        var group = await _addonService.UpdateGroupAsync(groupId, RequireTenantId(), request, ct);
        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}")]
    [RequiresPermission("catalog.products.delete")]
   
    public async Task<IActionResult> Delete(Guid groupId, CancellationToken ct)
    {
        await _addonService.DeleteGroupAsync(groupId, RequireTenantId(), ct);
        return DeletedResponse();
    }

    [HttpPost("{groupId:guid}/addons")]
    [RequiresPermission("catalog.products.update")]

    public async Task<IActionResult> AddAddon(Guid groupId, AddAddonToGroupRequest request, CancellationToken ct)
    {
        var addon = await _addonService.AddToGroupAsync(groupId, RequireTenantId(), request, ct);
        return CreatedResponse(addon);
    }

    [HttpPut("{groupId:guid}/addons/{addonId:guid}")]
    [RequiresPermission("catalog.products.update")]

    public async Task<IActionResult> UpdateAddon(Guid groupId, Guid addonId, AddAddonToGroupRequest request, CancellationToken ct)
    {
        var addon = await _addonService.UpdateAddonAsync(groupId, addonId, RequireTenantId(), request, ct);
        return OkResponse(addon);
    }

    [HttpDelete("{groupId:guid}/addons/{addonId:guid}")]
    [RequiresPermission("catalog.products.update")]

    public async Task<IActionResult> RemoveAddon(Guid groupId, Guid addonId, CancellationToken ct)
    {
        await _addonService.RemoveFromGroupAsync(groupId, addonId, RequireTenantId(), ct);
        return DeletedResponse();
    }
}