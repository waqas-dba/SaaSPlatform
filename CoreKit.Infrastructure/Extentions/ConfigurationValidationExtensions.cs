using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;

namespace CoreKit.Infrastructure.Extensions;

public static class ConfigurationValidationExtensions
{
    public static void ValidateCoreKitConfiguration(
        this WebApplicationBuilder builder)
    {
        var config = builder.Configuration;
        var env = builder.Environment;
        var errors = new List<string>();

        // ── JWT ───────────────────────────────────────────────────────────────
        var jwtSecret = config["Jwt:Secret"];
        if (string.IsNullOrWhiteSpace(jwtSecret))
        {
            errors.Add("Jwt:Secret is not configured.");
        }
        else
        {
            try
            {
                var bytes = Convert.FromBase64String(jwtSecret);
                if (bytes.Length < 32)
                    errors.Add(
                        $"Jwt:Secret is too short ({bytes.Length} bytes). " +
                        "Minimum is 32 bytes (256 bits). " +
                        "Generate with: openssl rand -base64 64");
            }
            catch
            {
                if (System.Text.Encoding.UTF8.GetByteCount(jwtSecret) < 32)
                    errors.Add(
                        "Jwt:Secret is too short. Minimum is 32 bytes.");
            }
        }

        // ── Encryption key ────────────────────────────────────────────────────
        var encKey = config["EncryptionKey"];
        if (string.IsNullOrWhiteSpace(encKey))
        {
            errors.Add("EncryptionKey is not configured.");
        }
        else
        {
            try
            {
                var bytes = Convert.FromBase64String(encKey);
                if (bytes.Length != 32)
                    errors.Add(
                        $"EncryptionKey must be exactly 32 bytes " +
                        $"(got {bytes.Length}). " +
                        "Generate with: openssl rand -base64 32");
            }
            catch
            {
                errors.Add("EncryptionKey is not valid base64.");
            }
        }

        // ── Connection string ─────────────────────────────────────────────────
        var connStr = config.GetConnectionString("Postgres");
        if (string.IsNullOrWhiteSpace(connStr))
            errors.Add("ConnectionStrings:Postgres is not configured.");

        // ── Production guards ─────────────────────────────────────────────────
        if (env.IsProduction())
        {
            if (connStr != null &&
                connStr.Contains(
                    "SSL Mode=Disable",
                    StringComparison.OrdinalIgnoreCase))
                errors.Add(
                    "SSL must not be disabled in production. " +
                    "Use 'SSL Mode=Require' in the connection string.");

            if (connStr != null &&
                connStr.Contains(
                    "Username=postgres",
                    StringComparison.OrdinalIgnoreCase))
                errors.Add(
                    "Do not connect as the 'postgres' superuser in production. " +
                    "Create a dedicated application database user.");

            if (jwtSecret != null &&
                jwtSecret == "k8!sJ3$pW9^tR2@zL7#mN5&bV8~qF4*hX6)d")
                errors.Add(
                    "JWT secret is the default example value. " +
                    "Rotate it immediately.");
        }

        if (errors.Count == 0) return;

        Console.ForegroundColor = ConsoleColor.Red;
        Console.WriteLine(
            "\n[CONFIG ERROR] Application cannot start " +
            "due to configuration problems:");
        foreach (var e in errors)
            Console.WriteLine($"  • {e}");
        Console.ResetColor();

        throw new InvalidOperationException(
            "Invalid configuration:\n" +
            string.Join("\n", errors.Select(e => "  • " + e)));
    }
}