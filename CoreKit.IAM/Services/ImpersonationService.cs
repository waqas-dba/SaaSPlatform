using CoreKit.IAM.Interfaces;
using CoreKit.SharedKernel.Exceptions;
using Microsoft.Extensions.Logging;

namespace CoreKit.IAM.Services;

public interface IImpersonationService
{
    Task<(string Token, DateTime ExpiresAt)> CreateImpersonationTokenAsync(
        Guid platformUserId,
        Guid targetTenantId,
        string reason);
}

public class ImpersonationService : IImpersonationService
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
        string reason)
    {
        // Verify platform user has impersonation permission
        var hasPermission = await _permissionService.HasPermissionAsync(
            platformUserId, null, Constants.Permissions.Platform.CreateImpersonation);

        if (!hasPermission)
            throw new ForbiddenException("You do not have permission to impersonate tenants.");

        // Get platform user
        var platformUser = await _userRepo.GetByIdAsync(platformUserId)
            ?? throw new KeyNotFoundException("Platform user not found.");

        // Create scoped token for target tenant only
        var (token, expiresAt, _) = _jwtService.GenerateAccessToken(
            platformUser,
            targetTenantId,
            new[] { "TenantAdmin" }, // Impersonation role
            permissions: null,       // Will be resolved per-tenant by permission service
            storeIds: null);         // Will be resolved per-tenant

        _logger.LogWarning(
            "Platform user {PlatformUserId} ({Email}) created impersonation token for tenant {TenantId}. Reason: {Reason}",
            platformUserId, platformUser.Email, targetTenantId, reason);

        return (token, expiresAt);
    }
}