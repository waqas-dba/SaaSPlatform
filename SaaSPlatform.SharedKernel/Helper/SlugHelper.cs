// SaaSPlatform.SharedKernel/Helper/SlugHelper.cs
using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreKit.SharedKernel.Helpers;

public static class SlugHelper
{
    // FIX: throw on empty input instead of silently returning empty string
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            throw new ArgumentException("Slug input must not be empty.", nameof(input));

        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');

        if (string.IsNullOrEmpty(slug))
        {
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input.Trim()));
            slug = Convert.ToHexString(hash)[..12].ToLowerInvariant();
        }

        return slug;
    }
}