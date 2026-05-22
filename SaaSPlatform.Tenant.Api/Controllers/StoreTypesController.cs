using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController]
[Route("api/storetypes")]
[Authorize]
public class StoreTypesController : ApiControllerBase
{
    private readonly IStoreTypeService _storeTypeService;

    public StoreTypesController(IStoreTypeService storeTypeService)
        => _storeTypeService = storeTypeService;

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var types = await _storeTypeService.GetAllActiveAsync();
        return Ok(types);
    }
}