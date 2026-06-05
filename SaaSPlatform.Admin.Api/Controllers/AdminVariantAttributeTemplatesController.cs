// SaaSPlatform.Admin.Api | Controllers/AdminVariantAttributeTemplatesController.cs
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
public class AdminVariantAttributeTemplatesController : ApiControllerBase
{
    private readonly IVariantAttributeTemplateService _templateService;

    public AdminVariantAttributeTemplatesController(IVariantAttributeTemplateService templateService)
        => _templateService = templateService;

    [HttpGet]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetAll([FromQuery] string? storeTypeCode, CancellationToken ct)
    {
        var templates = await _templateService.GetPlatformTemplatesAsync(storeTypeCode ?? string.Empty);
        return OkResponse(templates);
    }

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Create([FromBody] CreateVariantAttributeTemplateRequest request, CancellationToken ct)
    {
        var template = await _templateService.CreatePlatformTemplateAsync(request);
        return CreatedResponse(template);
    }

    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVariantAttributeTemplateRequest request, CancellationToken ct)
    {
        await _templateService.UpdatePlatformTemplateAsync(id, request);
        return UpdatedResponse();
    }

    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct)
    {
        await _templateService.DeletePlatformTemplateAsync(id);
        return DeletedResponse();
    }
}