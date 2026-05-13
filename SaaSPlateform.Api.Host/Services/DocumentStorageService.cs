using AuthCoreKit.IAM.Interfaces;
using SkiaSharp;

namespace SaaSPlatform.Api.Host.Services;

public class DocumentStorageService
{
    private readonly IEncryptionService _crypto;
    private readonly string _storageRoot;

    public DocumentStorageService(
        IEncryptionService crypto,
        IConfiguration config)
    {
        _crypto = crypto;

        _storageRoot = config["DocumentStoragePath"]
            ?? throw new InvalidOperationException(
                "DocumentStoragePath is missing.");

        Directory.CreateDirectory(_storageRoot);
    }

    public async Task<string> SaveAsync(
        Guid userId,
        string documentType,
        Stream fileStream)
    {
        // Copy uploaded stream
        using var input = new MemoryStream();

        await fileStream.CopyToAsync(input);

        input.Position = 0;

        // Decode image
        using var originalBitmap = SKBitmap.Decode(input);

        if (originalBitmap == null)
        {
            throw new InvalidOperationException(
                "Invalid image.");
        }

        SKBitmap finalBitmap = originalBitmap;

        // Resize if width > 1024
        if (originalBitmap.Width > 1024)
        {
            var ratio = 1024f / originalBitmap.Width;

            var width = 1024;
            var height = (int)(originalBitmap.Height * ratio);

            var resizedBitmap =
                new SKBitmap(width, height);

            using var canvas =
                new SKCanvas(resizedBitmap);

            // NEW SAFE APPROACH
            canvas.Scale(
                width / (float)originalBitmap.Width,
                height / (float)originalBitmap.Height);

            canvas.DrawBitmap(
                originalBitmap,
                0,
                0);

            finalBitmap = resizedBitmap;
        }

        // Compress JPEG
        using var image =
            SKImage.FromBitmap(finalBitmap);

        using var data =
            image.Encode(
                SKEncodedImageFormat.Jpeg,
                80);

        var plainBytes = data.ToArray();

        // Encrypt
        var encrypted =
            _crypto.Encrypt(plainBytes);

        // Create file path
        var fileName =
            $"{documentType}_{DateTime.UtcNow:yyyyMMddHHmmss}.enc";

        var relativePath =
            Path.Combine(
                userId.ToString(),
                fileName);

        var fullPath =
            Path.Combine(
                _storageRoot,
                relativePath);

        var directory =
            Path.GetDirectoryName(fullPath);

        if (!string.IsNullOrWhiteSpace(directory))
        {
            Directory.CreateDirectory(directory);
        }

        // Save encrypted file
        await File.WriteAllBytesAsync(
            fullPath,
            encrypted);

        // Dispose resized bitmap
        if (!ReferenceEquals(
            finalBitmap,
            originalBitmap))
        {
            finalBitmap.Dispose();
        }

        return relativePath;
    }

    public async Task<byte[]> ReadAsync(
        string relativePath)
    {
        var fullPath =
            Path.Combine(
                _storageRoot,
                relativePath);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException(
                "Encrypted document not found.",
                fullPath);
        }

        var encrypted =
            await File.ReadAllBytesAsync(fullPath);

        return _crypto.Decrypt(encrypted);
    }

    public void Delete(string relativePath)
    {
        var fullPath =
            Path.Combine(
                _storageRoot,
                relativePath);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }
}