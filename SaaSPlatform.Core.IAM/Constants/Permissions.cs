using System;
using System.Collections.Generic;
using System.Text;

namespace SaaSPlatform.Core.IAM.Constants;

public static class Permissions
{
    // Tenant
    public const string TenantView = "tenant.view";
    public const string TenantCreate = "tenant.create";
    public const string TenantApprove = "tenant.approve";
    public const string TenantReject = "tenant.reject";

    // Users
    public const string UserView = "user.view";
    public const string UserCreate = "user.create";
    public const string UserUpdate = "user.update";
    public const string UserDelete = "user.delete";

    // Billing
    public const string BillingView = "billing.view";
    public const string BillingManage = "billing.manage";
}
