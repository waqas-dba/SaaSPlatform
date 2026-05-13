using Microsoft.AspNetCore.Mvc;
using SaaSPlatform.Api.Host.Responses;

namespace SaaSPlatform.Api.Host.Controllers;

[ApiController]
public abstract class BaseApiController : ControllerBase
{
    protected IActionResult Success<T>(T data, string message = "")
        => Ok(ApiResponse<T>.SuccessResponse(data, message, HttpContext.TraceIdentifier));

    protected IActionResult Fail(
        string message,
        string? code = null,
        int status = 400,
        List<string>? errors = null)
        => StatusCode(status, ApiResponse<object>.FailResponse(
            message, code, errors, HttpContext.TraceIdentifier));

    protected Guid? TenantId => HttpContext.Items["TenantId"] as Guid?;
}