// SaaSPlatform.Tenant.Api/Controllers/VariantGroupsController.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/variant-groups")]
[Authorize]
public class VariantGroupsController : ApiControllerBase
{
    private readonly IVariantGroupService _variantGroupService;
    private readonly ITenantContext _tenantContext;

    public VariantGroupsController(
        IVariantGroupService variantGroupService,
        ITenantContext tenantContext)
    {
        _variantGroupService = variantGroupService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetAll()
    {
        var groups = await _variantGroupService
            .GetByTenantAsync(RequireTenantId());

        return OkResponse(groups);
    }

    [HttpGet("{groupId:guid}")]
    [RequiresPermission("catalog.products.view")]
    public async Task<IActionResult> GetById(Guid groupId)
    {
        var group = await _variantGroupService
            .GetByIdAsync(groupId, RequireTenantId());

        return group is null ? NotFound() : OkResponse(group);
    }

    [HttpPost]
    [RequiresPermission("catalog.products.create")]
    public async Task<IActionResult> Create(
        CreateVariantGroupRequest request)
    {
        var group = await _variantGroupService
            .CreateAsync(RequireTenantId(), request);

        return CreatedResponse(group);
    }

    [HttpPut("{groupId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> Update(
        Guid groupId,
        UpdateVariantGroupRequest request)
    {
        var group = await _variantGroupService
            .UpdateAsync(groupId, RequireTenantId(), request);

        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}")]
    [RequiresPermission("catalog.products.delete")]
    public async Task<IActionResult> Delete(Guid groupId)
    {
        await _variantGroupService
            .DeleteAsync(groupId, RequireTenantId());

        return DeletedResponse();
    }

    // ── Options ──────────────────────────────────────────────────────

    [HttpPost("{groupId:guid}/options")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> AddOption(
        Guid groupId,
        VariantGroupOptionRequest request)
    {
        var group = await _variantGroupService
            .AddOptionAsync(groupId, RequireTenantId(), request);

        return OkResponse(group);
    }

    [HttpPut("{groupId:guid}/options/{optionId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> UpdateOption(
        Guid groupId,
        Guid optionId,
        VariantGroupOptionRequest request)
    {
        var group = await _variantGroupService
            .UpdateOptionAsync(groupId, optionId, RequireTenantId(), request);

        return OkResponse(group);
    }

    [HttpDelete("{groupId:guid}/options/{optionId:guid}")]
    [RequiresPermission("catalog.products.update")]
    public async Task<IActionResult> RemoveOption(
        Guid groupId,
        Guid optionId)
    {
        var group = await _variantGroupService
            .RemoveOptionAsync(groupId, optionId, RequireTenantId());

        return OkResponse(group);
    }

    private Guid RequireTenantId() =>
        _tenantContext.TenantId
        ?? throw new UnauthorizedAccessException(
            "Tenant context is missing.");
}