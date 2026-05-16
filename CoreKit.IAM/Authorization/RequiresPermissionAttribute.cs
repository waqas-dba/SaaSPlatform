using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using CoreKit.IAM.Constants;
using CoreKit.IAM.Interfaces;
using CoreKit.IAM.Models;

namespace CoreKit.IAM.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public RequiresPermissionAttribute(string permission)
        {
            _permission = permission;
        }

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;

            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var options = context.HttpContext.RequestServices
                .GetRequiredService<IamOptions>();

            if (options.SuperAdminBypassPermissions &&
                user.IsInRole(options.SuperAdminRoleName))
            {
                return;
            }

            // Fast claim-based permission check
            var hasClaimPermission = user.Claims.Any(c =>
                c.Type == ClaimConstants.Permission &&
                c.Value == _permission);

            if (hasClaimPermission)
            {
                return;
            }

            // Fallback DB verification
            var permissionService = context.HttpContext.RequestServices
                .GetRequiredService<IPermissionService>();

            var currentUser = context.HttpContext.RequestServices
                .GetRequiredService<ICurrentUserService>();

            if (currentUser.UserId == null)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            // Resolve tenant scope for the permission check
            Guid? tenantId;
            try
            {
                var scope = currentUser.GetTenantScope();
                // For a non‑SuperAdmin user the scope must be tenant‑specific;
                // a global scope here would be an error (but we treat it as null).
                tenantId = scope.IsGlobal ? null : scope.TenantId;
            }
            catch (UnauthorizedAccessException)
            {
                // Tenant context could not be resolved – deny access
                context.Result = new ForbidResult();
                return;
            }

            var hasPermission = await permissionService.HasPermissionAsync(
                currentUser.UserId.Value,
                tenantId,
                _permission);

            if (!hasPermission)
            {
                context.Result = new ForbidResult();
            }
        }
    }
}