namespace SaaSPlatform.Core.Billing.Models;

public class SubscriptionFeatures
{
    public bool AllowOrders { get; set; } = true;
    public bool AllowProductCreation { get; set; } = true;
    public bool AllowAdminAccess { get; set; } = true;
}