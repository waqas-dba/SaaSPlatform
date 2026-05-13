using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Api.Host.Controllers.Admin;

[ApiController]
[Route("api/auth")]
public class AuthController : BaseApiController
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth)
    {
        _auth = auth;
    }

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login(LoginRequest request)
    {
        if (TenantId is null)
            return Fail("Missing tenant context");

        var result = await _auth.LoginAsync(request, TenantId.Value);
        return Success(result, "Login successful");
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh(RefreshTokenRequest request)
    {
        if (TenantId is null)
            return Fail("Missing tenant context");

        var result = await _auth.RefreshAsync(request, TenantId.Value);
        return Success(result, "Token refreshed");
    }
}