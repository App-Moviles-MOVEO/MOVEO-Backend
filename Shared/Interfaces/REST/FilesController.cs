using Microsoft.AspNetCore.Mvc;
using Moveo_backend.Shared.Infrastructure.Storage;
using Swashbuckle.AspNetCore.Annotations;

namespace Moveo_backend.Shared.Interfaces.REST;

[ApiController]
[Route("api/v1/files")]
[SwaggerTag("Subida genérica de archivos (imágenes/documentos)")]
public class FilesController(IFileStorageService fileStorage) : ControllerBase
{
    /// <summary>
    /// Sube uno o más archivos (multipart/form-data, campo "files") y devuelve sus URLs públicas.
    /// Reutilizable para vehículos, documentos e inspecciones cuando el flujo específico no aplique.
    /// </summary>
    [HttpPost]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(60_000_000)]
    [SwaggerResponse(StatusCodes.Status201Created, "Archivos subidos")]
    [SwaggerResponse(StatusCodes.Status400BadRequest, "Sin archivos o archivo inválido")]
    public async Task<IActionResult> Upload([FromForm] string? folder = null)
    {
        var files = Request.Form.Files;
        if (files.Count == 0)
            return BadRequest(new { error = "no_files", message = "Envíe al menos un archivo en el campo 'files'" });

        // Solo se permite un subfolder alfanumérico simple para evitar path traversal.
        var subfolder = string.IsNullOrWhiteSpace(folder) || !folder.All(char.IsLetterOrDigit)
            ? "files"
            : folder.ToLowerInvariant();

        var urls = new List<string>();
        foreach (var file in files)
        {
            if (!fileStorage.IsAllowedFile(file, out var error))
                return BadRequest(new { error = "invalid_file", message = error });
            urls.Add(await fileStorage.SaveAsync(file, $"{subfolder}/{DateTime.UtcNow:yyyyMM}", "file"));
        }

        return StatusCode(StatusCodes.Status201Created, new { urls });
    }
}
