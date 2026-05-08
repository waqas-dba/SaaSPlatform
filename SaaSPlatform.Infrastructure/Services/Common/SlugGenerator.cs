using SaaSPlatform.SharedKernel.Interfaces;

namespace SaaSPlatform.Infrastructure.Services.Common;

public class SlugGenerator : ISlugGenerator
{
    public string Generate(string input) =>
        input.ToLowerInvariant().Replace(" ", "-").Replace("'", "").Replace("\"", "");
}