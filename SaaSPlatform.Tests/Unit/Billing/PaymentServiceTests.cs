using Microsoft.EntityFrameworkCore;
using SaaSPlatform.Core.Billing.Entities;
using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Infrastructure.Persistence;
using SaaSPlatform.Infrastructure.Services.Billing;
using Xunit;

namespace SaaSPlatform.UnitTests.Billing;

public class PaymentServiceTests
{
    private SaaSPlatformDbContext GetDb()
    {
        var options = new DbContextOptionsBuilder<SaaSPlatformDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new SaaSPlatformDbContext(
            options,
            tenantContext: null,
            currentUser: null);
    }

    [Fact]
    public async Task Should_Mark_Payment_As_Paid()
    {
        var db = GetDb();

        var payment = new Payment
        {
            Id = Guid.NewGuid(),
            TenantId = Guid.NewGuid(),
            SubscriptionId = Guid.NewGuid(),
            Amount = 100,
            Status = PaymentStatus.Pending
        };

        db.Payments.Add(payment);

        await db.SaveChangesAsync();

        var service = new PaymentService(db);

        await service.MarkAsPaidAsync(payment.Id);

        var updated = await db.Payments.FirstAsync();

        Assert.Equal(PaymentStatus.Paid, updated.Status);

        Assert.NotNull(updated.PaidAt);
    }
}