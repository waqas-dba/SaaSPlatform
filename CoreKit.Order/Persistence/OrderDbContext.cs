// CoreKit.Order/Persistence/OrderDbContext.cs
using CoreKit.Order.Entities;
using CoreKit.SharedKernel.Common;
using CoreKit.SharedKernel.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CoreKit.Order.Persistence;

public class OrderDbContext : AuditableDbContext
{
    public OrderDbContext(DbContextOptions<OrderDbContext> options, ICurrentUser? currentUser = null)
        : base(options, currentUser) { }

    public DbSet<CustomerOrder> Orders => Set<CustomerOrder>();
    public DbSet<OrderItem> OrderItems => Set<OrderItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(OrderDbContext).Assembly);
    }
}