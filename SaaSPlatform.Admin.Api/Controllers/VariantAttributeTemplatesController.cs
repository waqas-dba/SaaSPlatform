using CoreKit.Catalog.Entities;
using CoreKit.Catalog.Persistence;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/catalog/variant-attribute-templates")]
[Authorize]
public class VariantAttributeTemplatesController : ApiControllerBase
{
    private readonly CatalogDbContext _db;

    public VariantAttributeTemplatesController(CatalogDbContext db) => _db = db;

    // -----------------------------------------------------------------------
    // GET ?storeTypeCode=restaurant
    // -----------------------------------------------------------------------
    [HttpGet]
    [RequiresPermission(Permissions.Catalog.TemplatesView)]
    public async Task<IActionResult> GetAll([FromQuery] string? storeTypeCode)
    {
        IQueryable<VariantAttributeTemplate> query = _db.VariantAttributeTemplates;

        if (!string.IsNullOrWhiteSpace(storeTypeCode))
            query = query.Where(t => t.StoreTypeCode == storeTypeCode);

        var templates = await query
            .OrderBy(t => t.StoreTypeCode)
            .ThenBy(t => t.Name)
            .ToListAsync();

        var result = templates.Select(t => new
        {
            t.Id,
            t.Name,
            t.StoreTypeCode,
            t.OptionsJson
        });

        return Ok(result);
    }

    // -----------------------------------------------------------------------
    // POST
    // -----------------------------------------------------------------------
    [HttpPost]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Create([FromBody] CreateVariantAttributeTemplateRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Name))
            return BadRequest("Name is required.");
        if (string.IsNullOrWhiteSpace(request.StoreTypeCode))
            return BadRequest("StoreTypeCode is required.");

        var template = new VariantAttributeTemplate
        {
            Id = Guid.NewGuid(),
            Name = request.Name.Trim(),
            StoreTypeCode = request.StoreTypeCode.Trim(),
            OptionsJson = request.OptionsJson?.Trim(),
            TenantId = null   // platform-level template
        };

        _db.VariantAttributeTemplates.Add(template);
        await _db.SaveChangesAsync();

        return CreatedResponse(new
        {
            template.Id,
            template.Name,
            template.StoreTypeCode,
            template.OptionsJson
        });
    }

    // -----------------------------------------------------------------------
    // PUT {id}
    // -----------------------------------------------------------------------
    [HttpPut("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateVariantAttributeTemplateRequest request)
    {
        var template = await _db.VariantAttributeTemplates.FindAsync(id);
        if (template == null) return NotFound();

        if (!string.IsNullOrWhiteSpace(request.Name))
            template.Name = request.Name.Trim();
        if (request.OptionsJson != null)               // allows clearing OptionsJson with empty string
            template.OptionsJson = string.IsNullOrWhiteSpace(request.OptionsJson)
                ? null
                : request.OptionsJson.Trim();

        await _db.SaveChangesAsync();
        return UpdatedResponse();
    }

    // -----------------------------------------------------------------------
    // DELETE {id}
    // -----------------------------------------------------------------------
    [HttpDelete("{id:guid}")]
    [RequiresPermission(Permissions.Catalog.TemplatesManagePlatform)]
    public async Task<IActionResult> Delete(Guid id)
    {
        var template = await _db.VariantAttributeTemplates.FindAsync(id);
        if (template == null) return NotFound();

        _db.VariantAttributeTemplates.Remove(template);
        await _db.SaveChangesAsync();
        return DeletedResponse();
    }
}

// ---------------------------------------------------------------------------
// Request DTOs
// ---------------------------------------------------------------------------
public record CreateVariantAttributeTemplateRequest(
    string Name,
    string StoreTypeCode,
    string? OptionsJson);

public record UpdateVariantAttributeTemplateRequest(
    string? Name,
    string? OptionsJson);