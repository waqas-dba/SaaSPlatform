using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.Extensions.DependencyInjection;
using AuthCoreKit.IAM.Interfaces;
using AuthCoreKit.IAM.Models;

namespace AuthCoreKit.IAM.Authorization
{
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Method, AllowMultiple = true)]
    public class RequiresPermissionAttribute : Attribute, IAsyncAuthorizationFilter
    {
        private readonly string _permission;

        public RequiresPermissionAttribute(string permission) => _permission = permission;

        public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
        {
            var user = context.HttpContext.User;
            if (user.Identity?.IsAuthenticated != true)
            {
                context.Result = new UnauthorizedResult();
                return;
            }

            var options = context.HttpContext.RequestServices.GetService<IamOptions>();
            if (options?.SuperAdminBypassPermissions == true && user.IsInRole(options.SuperAdminRoleName))
                return;

            var permissionService = context.HttpContext.RequestServices.GetRequiredService<IPermissionService>();
            var currentUserService = context.HttpContext.RequestServices.GetRequiredService<ICurrentUserService>();

            if (currentUserService.UserId == null)
            {
                context.Result = new UnauthorizedObjectResult("User not authenticated.");
                return;
            }

            var tenantId = currentUserService.TenantId;
            var hasPermission = await permissionService.HasPermissionAsync(
                currentUserService.UserId.Value, tenantId, _permission);

            if (!hasPermission)
                context.Result = new ForbidResult();
        }
    }
}