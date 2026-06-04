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
[Route("api/v{version:apiVersion}/admin/catalog/variant-attribute-templates")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class VariantAttributeTemplatesController : ApiControllerBase
{
    private readonly IVariantAttributeTemplateService _templateService;

    public VariantAttributeTemplatesController(IVariantAttributeTemplateService templateService)
        => _templateService = templateService;

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetAll([FromQuery] string? storeTypeCode, CancellationToken ct)
    {
        var templates = await _templateService.GetByStoreTypeAsync(storeTypeCode ?? string.Empty);
        return OkResponse(templates);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Create(
        [FromBody] CreateVariantAttributeTemplateRequest request,
        CancellationToken ct)
    {
        var template = await _templateService.CreateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Update(
        Guid id,
        [FromBody] UpdateVariantAttributeTemplateRequest request,
        CancellationToken ct)
    {
        await _templateService.UpdateAsync(id, request);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _templateService.DeleteAsync(id);
        return DeletedResponse();
    }
}