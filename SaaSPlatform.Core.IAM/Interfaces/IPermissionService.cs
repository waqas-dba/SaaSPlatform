using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Core.IAM.Interfaces
{
    public interface IPermissionService
    {
        Task<bool> HasPermissionAsync(Guid userId, Guid tenantId, string permission);
        Task<bool> HasModuleAccessAsync(Guid userId, Guid tenantId, string module);
    }
}
