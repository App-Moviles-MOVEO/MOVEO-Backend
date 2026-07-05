namespace Moveo_backend.Shared.Infrastructure.Storage;

/// <summary>
/// Implementación en disco local (wwwroot/uploads), servida por UseStaticFiles().
/// Mismo esquema que ya usa el KYC (/uploads/kyc/...). En Railway el disco es efímero:
/// si se necesita durabilidad ante redeploys, cambiar esta implementación por S3/Cloudinary
/// sin tocar los controllers.
/// </summary>
public class LocalFileStorageService(IWebHostEnvironment environment) : IFileStorageService
{
    private static readonly string[] AllowedExtensions =
        [".jpg", ".jpeg", ".png", ".webp", ".heic", ".pdf"];

    public const long MaxFileSizeBytes = 10_000_000; // 10 MB por archivo

    public bool IsAllowedFile(IFormFile file, out string? error)
    {
        error = null;
        if (file.Length == 0)
        {
            error = $"El archivo '{file.FileName}' está vacío";
            return false;
        }
        if (file.Length > MaxFileSizeBytes)
        {
            error = $"El archivo '{file.FileName}' supera el máximo de 10 MB";
            return false;
        }
        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!string.IsNullOrEmpty(ext) && !AllowedExtensions.Contains(ext))
        {
            error = $"Extensión no permitida '{ext}'. Permitidas: {string.Join(", ", AllowedExtensions)}";
            return false;
        }
        return true;
    }

    public async Task<string> SaveAsync(IFormFile file, string subfolder, string prefix)
    {
        var webRoot = environment.WebRootPath
                      ?? Path.Combine(environment.ContentRootPath, "wwwroot");
        var safeSubfolder = subfolder.Trim('/');
        var folder = Path.Combine(webRoot, "uploads",
            safeSubfolder.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        var fileName = $"{prefix}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        var relative = $"/uploads/{safeSubfolder}/{fileName}";
        var baseUrl = Environment.GetEnvironmentVariable("PUBLIC_BASE_URL");
        return string.IsNullOrWhiteSpace(baseUrl) ? relative : baseUrl.TrimEnd('/') + relative;
    }
}
