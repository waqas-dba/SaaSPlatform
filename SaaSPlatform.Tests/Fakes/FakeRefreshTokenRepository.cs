using AuthCoreKit.IAM.Entities;
using AuthCoreKit.IAM.Interfaces;

namespace SaaSPlatform.UnitTests.Fakes;

public class FakeRefreshTokenRepository
    : IRefreshTokenRepository
{
    private readonly List<RefreshToken> _tokens = [];

    public Task AddAsync(RefreshToken token)
    {
        _tokens.Add(token);

        return Task.CompletedTask;
    }

    public Task<RefreshToken?> GetByTokenAsync(string token)
    {
        var refreshToken =
            _tokens.FirstOrDefault(x => x.Token == token);

        return Task.FromResult(refreshToken);
    }

    public Task UpdateAsync(RefreshToken token)
    {
        return Task.CompletedTask;
    }
}