using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Controllers;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (TenantId is null)
            return Fail("Missing tenant context");

        var result = await _authService.LoginAsync(request, TenantId.Value);

        return Success(result, "Login successful");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        if (TenantId is null)
            return Fail("Missing tenant context");

        var result = await _authService.RefreshAsync(request, TenantId.Value);

        return Success(result, "Token refreshed");
    }
}