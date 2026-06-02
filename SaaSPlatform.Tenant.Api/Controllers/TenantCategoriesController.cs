// SaaSPlatform.Tenant.Api/Controllers/TenantCategoriesController.cs
using CoreKit.Catalog.Interfaces;
using CoreKit.Catalog.Models;
using CoreKit.IAM.Authorization;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/categories")]
[Authorize]
public class TenantCategoriesController : ApiControllerBase
{
    private readonly ICategoryService _categoryService;
    private readonly ITenantContext _tenantContext;

    public TenantCategoriesController(ICategoryService categoryService, ITenantContext tenantContext)
    {
        _categoryService = categoryService;
        _tenantContext = tenantContext;
    }

    [HttpGet]
    [RequiresPermission("catalog.categories.view")]
    public async Task<IActionResult> GetForTenant([FromQuery] Guid? storeId)
    {
        if (!_tenantContext.TenantId.HasValue)
            return Unauthorized(new { success = false, errorCode = "UNAUTHORIZED", message = "Missing tenant context." });

        var categories = await _categoryService.GetByTenantAsync(_tenantContext.TenantId.Value, storeId);
        return OkResponse(categories);
    }

    [HttpPost]
    [RequiresPermission("catalog.categories.create")]
    public async Task<IActionResult> Create(CreateCategoryRequest request)
    {
        request.TenantId = _tenantContext.TenantId ?? throw new UnauthorizedAccessException("Missing tenant context.");
        var category = await _categoryService.CreateAsync(request);
        return CreatedResponse(category);
    }
}