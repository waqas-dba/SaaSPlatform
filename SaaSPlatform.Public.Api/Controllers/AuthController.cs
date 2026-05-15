using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.Tenant.Abstractions;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Public.Api.Controllers;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly ITenantContext _tenantContext;

    public AuthController(
        IAuthService authService,
        ITenantContext tenantContext)
    {
        _authService = authService;
        _tenantContext = tenantContext;
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, _tenantContext.TenantId);
        return Ok(result);
    }

    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request, _tenantContext.TenantId);
        return Ok(result);   // LoginResponse with accessToken + refreshToken
    }

    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            return BadRequest("Refresh token is required.");

        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }
}