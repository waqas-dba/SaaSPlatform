using Microsoft.AspNetCore.Http;

namespace SaaSPlatform.BuildingBlocks.MultiTenancy;

public class TenantProvider : ITenantProvider
{
    private readonly IHttpContextAccessor _http;

    public TenantProvider(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid GetTenantId()
    {
        return (Guid)_http.HttpContext!.Items["TenantId"];
    }
}