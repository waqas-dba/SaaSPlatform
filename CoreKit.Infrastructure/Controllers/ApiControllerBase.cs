using CoreKit.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace CoreKit.Infrastructure.Controllers;

[ApiController]
public abstract class ApiControllerBase : ControllerBase
{
    protected IActionResult OkResponse(string? message = null)
        => Ok(ApiResponse.Ok(message));

    protected IActionResult OkResponse<T>(T data, string? message = null)
        => Ok(ApiResponse.Ok(data, message));

    protected IActionResult DeletedResponse(string? message = null)
        => Ok(ApiResponse.Ok(message ?? "Deleted successfully."));

    protected IActionResult UpdatedResponse(string? message = null)
        => Ok(ApiResponse.Ok(message ?? "Updated successfully."));

    protected IActionResult CreatedResponse<T>(T data, string? message = null)
        => Ok(ApiResponse.Ok(data, message ?? "Created successfully."));
}