using SaaSPlatform.Core.Billing.Enums;
using SaaSPlatform.Core.Billing.Interfaces;
using SaaSPlatform.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace SaaSPlatform.Infrastructure.Services.Billing;

public class PaymentService : IPaymentService
{
    private readonly SaaSPlatformDbContext _db;

    public PaymentService(SaaSPlatformDbContext db)
    {
        _db = db;
    }

    public async Task MarkAsPaidAsync(Guid paymentId)
    {
        var payment = await _db.Payments.FindAsync(paymentId);

        if (payment == null)
            return;

        payment.Status = PaymentStatus.Paid;
        payment.PaidAt = DateTime.UtcNow;
        payment.StatusUpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }
}