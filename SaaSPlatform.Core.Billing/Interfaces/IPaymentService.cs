namespace SaaSPlatform.Core.Billing.Interfaces;

public interface IPaymentService
{
    Task MarkAsPaidAsync(Guid paymentId);
}