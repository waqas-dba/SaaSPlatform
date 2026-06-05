// CoreKit.Order/Persistence/Seeders/OrderSeeder.cs
using CoreKit.Order.Entities;
using CoreKit.Order.Persistence;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Order.Persistence.Seeders;

public sealed class OrderSeeder
{
    private readonly OrderDbContext _db;

    public OrderSeeder(OrderDbContext db) => _db = db;

    public async Task SeedAsync(CancellationToken ct = default)
    {
        if (await _db.Orders.AnyAsync(ct)) return;

        // System store ID (from TenantSeeder)
        var systemStoreId = Guid.Parse("22222222-3333-4444-5555-666666666666");
        var systemTenantId = Guid.Parse("11111111-1111-1111-1111-111111111111");

        // ----- Collection Order -----
        var collectionOrder = new CustomerOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-20260605-COL001",
            StoreId = systemStoreId,
            TenantId = systemTenantId,
            CustomerName = "Alice Walker",
            CustomerPhone = "03001234567",
            Type = OrderType.Collection,
            Status = OrderStatus.Pending,
            SubTotal = 120.00m,
            Total = 120.00m,
            Notes = "Please pack in a paper bag.",
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(), // placeholder – use a real product ID from your catalog seeder
                    ProductName = "Sourdough Loaf",
                    Quantity = 2,
                    UnitPrice = 60.00m,
                }
            }
        };

        // ----- Delivery Order -----
        var deliveryOrder = new CustomerOrder
        {
            Id = Guid.NewGuid(),
            OrderNumber = "ORD-20260605-DEL001",
            StoreId = systemStoreId,
            TenantId = systemTenantId,
            CustomerName = "Bob Smith",
            CustomerPhone = "03007654321",
            Type = OrderType.Delivery,
            Status = OrderStatus.Confirmed,
            DeliveryAddress = "123 Main Street, Karachi",
            DeliveryFee = 50.00m,
            SubTotal = 350.00m,
            Total = 400.00m,
            Notes = null,
            Items = new List<OrderItem>
            {
                new()
                {
                    Id = Guid.NewGuid(),
                    ProductId = Guid.NewGuid(), // placeholder
                    ProductName = "Sourdough Loaf",
                    Quantity = 1,
                    UnitPrice = 350.00m,
                }
            }
        };

        _db.Orders.AddRange(collectionOrder, deliveryOrder);
        await _db.SaveChangesAsync(ct);
    }
}