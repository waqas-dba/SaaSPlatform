// CoreKit.SharedKernel/Helper/SlugHelper.cs

using System.Security.Cryptography;
using System.Text;
using System.Text.RegularExpressions;

namespace CoreKit.SharedKernel.Helpers;

public static class SlugHelper
{
    public static string Generate(string input)
    {
        if (string.IsNullOrWhiteSpace(input)) return string.Empty;

        var slug = input.Trim().ToLowerInvariant();
        slug = Regex.Replace(slug, @"[^a-z0-9\s-]", "");
        slug = Regex.Replace(slug, @"\s+", "-");
        slug = slug.Trim('-');

        // Non-ASCII input (Arabic, Urdu, Chinese, etc.) strips to empty after
        // the regex pass. Fall back to a 12-char hex token derived from the
        // original UTF-8 bytes so the DB unique constraint is never violated
        // with an empty string, and the slug is still deterministic.
        if (string.IsNullOrEmpty(slug))
        {
            var hash = SHA1.HashData(Encoding.UTF8.GetBytes(input.Trim()));
            slug = Convert.ToHexString(hash)[..12].ToLowerInvariant();
        }

        return slug;
    }
}