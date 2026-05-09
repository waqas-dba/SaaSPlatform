using SaaSPlatform.SharedKernel.Interfaces;

namespace SaaSPlatform.Infrastructure.Services.Common;

public class SlugGenerator : ISlugGenerator
{
    public string Generate(string input)
    {
        var baseSlug = input.ToLowerInvariant()
                           .Replace(" ", "-")
                           .Replace("'", "")
                           .Replace("\"", "");
        return baseSlug + "-" + Guid.NewGuid().ToString("N")[..6]; // ensures uniqueness
    }
}