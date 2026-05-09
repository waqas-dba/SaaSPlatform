using System.Text.Json;
using SaaSPlatform.Api.Host.Responses;
using SaaSPlatform.Core.Tenant.Interfaces;

namespace SaaSPlatform.Api.Host.Middleware
{
    public class TenantMiddleware
    {
        private readonly RequestDelegate _next;

        public TenantMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, ITenantContext tenantContext)
        {
            var path = context.Request.Path.Value?.ToLower();

            // Allow Swagger and tenant registration without tenant header
            if (path!.StartsWith("/swagger") || path.StartsWith("/api/tenants/register"))
            {
                await _next(context);
                return;
            }

            var tenantHeader = context.Request.Headers["x-tenant-id"].FirstOrDefault();
            if (!Guid.TryParse(tenantHeader, out var tenantId))
            {
                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";

                var error = ApiResponse<object>.FailResponse(
                    message: "Missing or invalid tenant context. Include 'x-tenant-id' header.",
                    code: "MISSING_TENANT",
                    traceId: context.TraceIdentifier);

                await context.Response.WriteAsync(JsonSerializer.Serialize(error));
                return;
            }

            tenantContext.SetTenantId(tenantId);
            context.Items["TenantId"] = tenantId;

            await _next(context);
        }
    }
}