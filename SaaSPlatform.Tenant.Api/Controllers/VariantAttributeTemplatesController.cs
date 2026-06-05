// SaaSPlatform.Tenant.Api/Controllers/VariantAttributeTemplatesController.cs
using Asp.Versioning;
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;   // correct namespace

[ApiController]
[Route("api/v{version:apiVersion}/tenant/catalog/variant-attribute-templates")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class VariantAttributeTemplatesController : TenantApiControllerBase
{
    private readonly IVariantAttributeTemplateService _service;

    public VariantAttributeTemplatesController(
        IVariantAttributeTemplateService service,
        ITenantContext tenantContext) : base(tenantContext)
        => _service = service;

    [HttpGet("platform/{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetPlatform(string storeTypeCode, CancellationToken ct)
        => OkResponse(await _service.GetPlatformTemplatesAsync(storeTypeCode));

    [HttpGet("{storeTypeCode}")]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetTenant(string storeTypeCode, CancellationToken ct)
        => OkResponse(await _service.GetTenantTemplatesAsync(RequireTenantId(), storeTypeCode));

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVariantAttributeTemplateRequest request,
        CancellationToken ct)
    {
        var result = await _service.CreateTenantTemplateAsync(RequireTenantId(), request);
        return CreatedResponse(result);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVariantAttributeTemplateRequest request,
        CancellationToken ct)
    {
        await _service.UpdateTenantTemplateAsync(id, RequireTenantId(), request);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManageTenant)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _service.DeleteTenantTemplateAsync(id, RequireTenantId());
        return DeletedResponse();
    }
}