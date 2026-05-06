namespace SaaSPlatform.BuildingBlocks.MultiTenancy;

public interface ITenantProvider
{
    Guid GetTenantId();
}