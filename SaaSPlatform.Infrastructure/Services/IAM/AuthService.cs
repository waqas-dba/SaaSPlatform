using SaaSPlatform.Core.IAM.Interfaces;
using SaaSPlatform.Core.IAM.Models;
using SaaSPlatform.Core.IAM.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtTokenService _jwtService;

    public AuthService(
        IUserRepository userRepository,
        IJwtTokenService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, Guid tenantId)
    {
        var phone = PhoneNormalizer.Normalize(request.Phone);

        var user = await _userRepository.GetByPhoneNumberAsync(phone);

        if (user == null)
            throw new UnauthorizedAccessException("Invalid credentials");

        if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials");

        var roles = user.UserRoles.Select(r => r.Role.Name);

        var (token, expiresAt) = _jwtService.GenerateAccessToken(
            user,
            tenantId,
            roles);

        return new LoginResponse
        {
            AccessToken = token,
            ExpiresAt = expiresAt
        };
    }
}