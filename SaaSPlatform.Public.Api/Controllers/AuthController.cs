using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Tenant.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace SaaSPlatform.Public.Api.Controllers;

[ApiController]
[Route("api/auth")]
//[EnableRateLimiting("login")]
public class AuthController : ApiControllerBase
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
    [AllowAnonymous]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        var result = await _authService.LoginAsync(request, _tenantContext.TenantId);
        return Ok(result);
    }

    [HttpPost("refresh")]
    [AllowAnonymous]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        var result = await _authService.RefreshAsync(request, _tenantContext.TenantId);
        return Ok(result);
    }

    // LOW FIX — require authentication so only the token's owner can revoke it
    [HttpPost("logout")]
    [Authorize]
    public async Task<IActionResult> Logout([FromBody] LogoutRequest request)
    {
        if (string.IsNullOrWhiteSpace(request?.RefreshToken))
            return BadRequest("Refresh token is required.");

        await _authService.LogoutAsync(request.RefreshToken);
        return Ok(new { message = "Logged out successfully" });
    }
}