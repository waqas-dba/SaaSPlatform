using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Subscription.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Admin.Api.Controllers;

[ApiController]
[Route("api/admin/plans")]
[Authorize]
public class PlansController : ApiControllerBase
{
    private readonly IPlanService _planService;

    public PlansController(IPlanService planService) => _planService = planService;

    [HttpGet]
    [RequiresPermission(Permissions.Platform.ManageSystemSettings)]
    public async Task<IActionResult> GetAll()
    {
        var plans = await _planService.GetAllAsync();
        return Ok(plans.Select(p => new { p.Id, p.Name, p.Code, p.MonthlyPrice, p.MaxStores, p.MaxProducts, p.CustomDomainEnabled, p.ThemeCustomizationEnabled }));
    }

    [HttpPost]
    [RequiresPermission(Permissions.Platform.ManageSystemSettings)]
    public async Task<IActionResult> Create([FromBody] CreatePlanRequest request)
    {
        var plan = await _planService.CreateAsync(
            request.Name, request.Code, request.Description,
            request.MonthlyPrice, request.YearlyPrice,
            request.MaxStores, request.MaxProducts, request.MaxCategories,
            request.CustomDomainEnabled, request.ThemeCustomizationEnabled,
            request.SortOrder);
        return CreatedResponse(new { plan.Id, plan.Name });
    }
}

public class CreatePlanRequest
{
    public string Name { get; set; } = default!;
    public string Code { get; set; } = default!;
    public string? Description { get; set; }
    public decimal MonthlyPrice { get; set; }
    public decimal YearlyPrice { get; set; }
    public int? MaxStores { get; set; }
    public int? MaxProducts { get; set; }
    public int? MaxCategories { get; set; }
    public bool CustomDomainEnabled { get; set; }
    public bool ThemeCustomizationEnabled { get; set; }
    public int SortOrder { get; set; } = 99;
}