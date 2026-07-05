namespace Moveo_backend.Shared.Infrastructure.Storage;

/// <summary>
/// Almacenamiento de archivos subidos por las apps (imágenes de vehículos, documentos,
/// inspecciones). Guarda bajo wwwroot/uploads y devuelve la URL pública del archivo.
/// </summary>
public interface IFileStorageService
{
    /// <summary>
    /// Guarda el archivo en uploads/{subfolder}/ y devuelve su URL.
    /// Si la variable de entorno PUBLIC_BASE_URL está configurada la URL es absoluta;
    /// si no, es relativa al host de la API (ej. "/uploads/vehicles/2/img_abc.jpg").
    /// </summary>
    Task<string> SaveAsync(IFormFile file, string subfolder, string prefix);

    /// <summary>Valida extensión y tamaño para archivos de imagen/documento.</summary>
    bool IsAllowedFile(IFormFile file, out string? error);
}
