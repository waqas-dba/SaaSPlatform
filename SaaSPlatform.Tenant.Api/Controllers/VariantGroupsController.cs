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
[Route("api/v{version:apiVersion}/variant-groups")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class VariantGroupsController : TenantApiControllerBase
{
    private readonly IVariantGroupService _variantGroupService;

    public VariantGroupsController(
        IVariantGroupService variantGroupService,
        ITenantContext tenantContext) : base(tenantContext)
    {
        _variantGroupService = variantGroupService;
    }

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var groups = await _variantGroupService.GetByTenantAsync(RequireTenantId(), ct);
        return OkResponse(groups);
    }

    [HttpGet("{groupId:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid groupId, CancellationToken ct)
    {
        var group = await _variantGroupService.GetByIdAsync(groupId, RequireTenantId(), ct);
        return group is null ? NotFound() : OkResponse(group);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.create")]
    public async Task<IActionResult> Create(CreateVariantGroupRequest request, CancellationToken ct)
    {
        var group = await _variantGroupService.CreateAsync(RequireTenantId(), request, ct);
        return CreatedResponse(group);
    }

    [HttpPut("{groupId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Update(Guid groupId, UpdateVariantGroupRequest request, CancellationToken ct)
    {
        var group = await _variantGroupService.UpdateAsync(groupId, RequireTenantId(), request, ct);
        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}")]
    [RequiresPermission("catalog.products.delete")]
    public async Task<IActionResult> Delete(Guid groupId, CancellationToken ct)
    {
        await _variantGroupService.DeleteAsync(groupId, RequireTenantId(), ct);
        return DeletedResponse();
    }

    [HttpPost("{groupId:guid}/options")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> AddOption(Guid groupId, VariantGroupOptionRequest request, CancellationToken ct)
    {
        var group = await _variantGroupService.AddOptionAsync(groupId, RequireTenantId(), request, ct);
        return OkResponse(group);
    }

    [HttpPut("{groupId:guid}/options/{optionId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> UpdateOption(Guid groupId, Guid optionId, VariantGroupOptionRequest request, CancellationToken ct)
    {
        var group = await _variantGroupService.UpdateOptionAsync(groupId, optionId, RequireTenantId(), request, ct);
        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}/options/{optionId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> RemoveOption(Guid groupId, Guid optionId, CancellationToken ct)
    {
        var group = await _variantGroupService.RemoveOptionAsync(groupId, optionId, RequireTenantId(), ct);
        return OkResponse(group);
    }
}