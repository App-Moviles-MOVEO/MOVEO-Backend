namespace Moveo_backend.Support.Domain.Model.Aggregate;

/// <summary>
/// Solicitud de alianza corporativa (US46). Se evalúa automáticamente al crearse:
/// se aprueba con RUC válido (11 dígitos) y flota ≥ 5 unidades; en caso contrario queda en revisión.
/// </summary>
public class Partnership
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string CompanyName { get; private set; } = string.Empty;
    public string Ruc { get; private set; } = string.Empty;
    public string ContactName { get; private set; } = string.Empty;
    public string? ContactEmail { get; private set; }
    public string? ContactPhone { get; private set; }
    public int FleetSize { get; private set; }
    public string Status { get; private set; } = "pending"; // pending | approved | rejected
    public string? ReviewNote { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ReviewedAt { get; private set; }

    protected Partnership() { }

    public Partnership(int userId, string companyName, string ruc, string contactName,
        string? contactEmail, string? contactPhone, int fleetSize)
    {
        UserId = userId;
        CompanyName = companyName;
        Ruc = ruc;
        ContactName = contactName;
        ContactEmail = contactEmail;
        ContactPhone = contactPhone;
        FleetSize = fleetSize;
        CreatedAt = DateTime.UtcNow;
        AutoEvaluate();
    }

    /// <summary>Evaluación automática (misma regla que la app: RUC de 11 dígitos y flota ≥ 5).</summary>
    public void AutoEvaluate()
    {
        var rucValid = !string.IsNullOrWhiteSpace(Ruc) && Ruc.Length == 11 && Ruc.All(char.IsDigit);
        if (rucValid && FleetSize >= 5)
        {
            Status = "approved";
            ReviewNote = "Aprobado automáticamente (RUC válido y flota suficiente)";
            ReviewedAt = DateTime.UtcNow;
        }
        else
        {
            Status = "pending";
            ReviewNote = rucValid
                ? "En revisión: flota menor a 5 unidades"
                : "En revisión: RUC no válido (se requieren 11 dígitos)";
        }
    }

    public void Resolve(string status, string? note)
    {
        Status = status == "approved" ? "approved" : status == "rejected" ? "rejected" : Status;
        ReviewNote = note ?? ReviewNote;
        ReviewedAt = DateTime.UtcNow;
    }
}
