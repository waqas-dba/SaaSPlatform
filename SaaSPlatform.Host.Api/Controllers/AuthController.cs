using Microsoft.AspNetCore.Mvc;
using CoreKit.Contracts.Interfaces;
using CoreKit.Contracts.Models;
using CoreKit.Tenant.Abstractions;

namespace SaaSPlatform.Host.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;
    private readonly ITenantContext _tenantContext;

    public AuthController(IAuthService auth, ITenantContext tenantContext)
    {
        _auth = auth;
        _tenantContext = tenantContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var response = await _auth.LoginAsync(request, _tenantContext.TenantId);
        return Ok(response);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var response = await _auth.RefreshAsync(request, _tenantContext.TenantId);
        return Ok(response);
    }
}