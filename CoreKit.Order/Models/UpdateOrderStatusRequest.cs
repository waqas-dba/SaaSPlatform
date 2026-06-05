// CoreKit.Order/Models/UpdateOrderStatusRequest.cs
namespace CoreKit.Order.Models;

public class UpdateOrderStatusRequest
{
    public string Status { get; set; } = default!;
}