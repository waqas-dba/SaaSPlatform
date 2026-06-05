// CoreKit.IAM/Services/ImpersonationService.cs
using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace CoreKit.IAM.Services;

public sealed class ImpersonationService : IImpersonationService
{
    private readonly IJwtTokenService _jwtService;
    private readonly IUserRepository _userRepo;
    private readonly IPermissionService _permissionService;
    private readonly ILogger<ImpersonationService> _logger;

    public ImpersonationService(
        IJwtTokenService jwtService,
        IUserRepository userRepo,
        IPermissionService permissionService,
        ILogger<ImpersonationService> logger)
    {
        _jwtService = jwtService;
        _userRepo = userRepo;
        _permissionService = permissionService;
        _logger = logger;
    }

    public async Task<(string Token, DateTime ExpiresAt)> CreateImpersonationTokenAsync(
        Guid platformUserId,
        Guid targetTenantId,
        string reason,
        CancellationToken ct = default)
    {
        if (string.IsNullOrWhiteSpace(reason))
            throw new ArgumentException(
                "Impersonation reason is required.", nameof(reason));

        var hasPermission = await _permissionService.HasPermissionAsync(
            platformUserId, null,
            Constants.Permissions.Platform.CreateImpersonation);

        if (!hasPermission)
            throw new ForbiddenException(
                "You do not have permission to impersonate tenants.");

        var platformUser = await _userRepo.GetByIdAsync(platformUserId)
            ?? throw new KeyNotFoundException("Platform user not found.");

        var tenantPermissions = await _permissionService
            .GetUserPermissionsAsync(platformUserId, targetTenantId);

        var scopedPermissions = tenantPermissions
            .Where(p => !p.StartsWith("platform.", StringComparison.Ordinal))
            .ToList();

        // FIX CS8130/CS8183: use explicit typed variables instead of deconstruction
        // so the compiler can resolve the tuple members unambiguously
        var result = _jwtService.GenerateAccessToken(
            platformUser,
            targetTenantId,
            roles: new[] { "TenantAdmin" },
            permissions: scopedPermissions,
            storeIds: null);

        string token = result.Token;
        DateTime expiresAt = result.ExpiresAt;

        _logger.LogWarning(
            "Platform user {PlatformUserId} ({Email}) created impersonation token " +
            "for tenant {TenantId}. Reason: {Reason}. Permissions granted: {Count}",
            platformUserId,
            platformUser.Email,
            targetTenantId,
            reason,
            scopedPermissions.Count);

        return (token, expiresAt);
    }
}