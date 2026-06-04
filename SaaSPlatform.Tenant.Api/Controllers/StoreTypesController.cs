using Asp.Versioning;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/v{version:apiVersion}/storetypes")]
[Authorize]
[Asp.Versioning.ApiVersion("1.0")]
public class StoreTypesController : ApiControllerBase
{
    private readonly IStoreTypeService _storeTypeService;

    public StoreTypesController(IStoreTypeService storeTypeService)
        => _storeTypeService = storeTypeService;

    [HttpGet]
    public async Task<IActionResult> GetAll(CancellationToken ct)
    {
        var types = await _storeTypeService.GetAllActiveAsync(ct);
        return OkResponse(types);
    }
}