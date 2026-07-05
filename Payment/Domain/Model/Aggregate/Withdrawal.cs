namespace Moveo_backend.Payment.Domain.Model.Aggregate;

/// <summary>
/// Solicitud de retiro de fondos del wallet de un usuario (owner). El dinero disponible
/// proviene de los pagos recibidos; al solicitar un retiro se retiene del balance hasta
/// que un proceso externo lo marque completado o rechazado.
/// </summary>
public class Withdrawal
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public decimal Amount { get; private set; }
    public string Method { get; private set; } = "yape";       // "yape" | "plin" | "bank"
    public string Destination { get; private set; } = string.Empty; // número/cuenta destino
    public string Status { get; private set; } = "pending";     // pending | completed | rejected
    public string? RejectionReason { get; private set; }
    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime? ProcessedAt { get; private set; }

    protected Withdrawal() { }

    public Withdrawal(int userId, decimal amount, string method, string destination)
    {
        UserId = userId;
        Amount = amount;
        Method = string.IsNullOrWhiteSpace(method) ? "yape" : method;
        Destination = destination;
        Status = "pending";
        CreatedAt = DateTime.UtcNow;
    }

    public void Complete()
    {
        Status = "completed";
        ProcessedAt = DateTime.UtcNow;
    }

    public void Reject(string? reason)
    {
        Status = "rejected";
        RejectionReason = reason;
        ProcessedAt = DateTime.UtcNow;
    }
}
