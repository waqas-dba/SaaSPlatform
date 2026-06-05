using Asp.Versioning;
using CoreKit.IAM.Authorization;
using CoreKit.IAM.Constants;
using CoreKit.Infrastructure.Controllers;
using CoreKit.Order.Interfaces;
using CoreKit.Order.Models;
using CoreKit.SharedKernel.Models;
using CoreKit.SharedKernel.Tenancy;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace SaaSPlatform.Tenant.Api.Controllers;

[ApiController, Route("api/v{version:apiVersion}/orders"), Authorize, Asp.Versioning.ApiVersion("1.0")]
public class OrdersController : TenantApiControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService, ITenantContext tenantContext)
        : base(tenantContext) => _orderService = orderService;

    [HttpPost]
    [RequiresPermission(Permissions.Catalog.OrdersCreate)]
    public async Task<IActionResult> Create(CreateOrderRequest request, CancellationToken ct)
    {
        var order = await _orderService.CreateAsync(request, ct);
        return CreatedResponse(order);
    }

    [HttpGet("{orderId:guid}")]
    [RequiresPermission(Permissions.Catalog.OrdersView)]
    public async Task<IActionResult> GetById(Guid orderId, CancellationToken ct)
    {
        var order = await _orderService.GetByIdAsync(orderId, ct);
        return order is null ? NotFound() : OkResponse(order);
    }

    [HttpGet("store/{storeId:guid}")]
    [RequiresPermission(Permissions.Catalog.OrdersView)]
    public async Task<IActionResult> GetByStore(Guid storeId, [FromQuery] PagedQuery query, CancellationToken ct)
    {
        var result = await _orderService.GetByStoreAsync(storeId, query, ct);
        return OkResponse(result);
    }

    [HttpPut("{orderId:guid}/status")]
    [RequiresPermission(Permissions.Catalog.OrdersUpdate)]
    public async Task<IActionResult> UpdateStatus(Guid orderId, UpdateOrderStatusRequest request, CancellationToken ct)
    {
        await _orderService.UpdateStatusAsync(orderId, request, ct);
        return UpdatedResponse();
    }

    [HttpDelete("{orderId:guid}")]
    [RequiresPermission(Permissions.Catalog.OrdersDelete)]
    public async Task<IActionResult> Cancel(Guid orderId, CancellationToken ct)
    {
        await _orderService.CancelAsync(orderId, ct);
        return DeletedResponse("Order cancelled.");
    }
}