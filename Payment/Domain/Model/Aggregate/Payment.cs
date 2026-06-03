using Moveo_backend.Payment.Domain.Model.Commands;

namespace Moveo_backend.Payment.Domain.Model.Aggregate;

/// <summary>
///     Payment Aggregate Root
/// </summary>
public class Payment
{
    public int Id { get; }
    public int PayerId { get; private set; }
    public int RecipientId { get; private set; }
    public int RentalId { get; private set; }
    public decimal Amount { get; private set; }
    public string Currency { get; private set; } // "PEN" | "USD"
    public string Method { get; private set; } // "card" | "yape" | "plin" | "transfer"
    public string Status { get; private set; } // "pending" | "completed" | "failed" | "refunded"
    public string? TransactionId { get; private set; }
    public string Type { get; private set; } // "rental" | "deposit" | "commission" | "refund"
    public string? Description { get; private set; }
    public string? Reason { get; private set; }
    public DateTime? DueDate { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime? CompletedAt { get; private set; }

    public Payment()
    {
        Currency = "PEN";
        Method = string.Empty;
        Status = "pending";
        Type = string.Empty;
        CreatedAt = DateTime.UtcNow;
    }

    public Payment(CreatePaymentCommand command) : this()
    {
        PayerId = command.PayerId;
        RecipientId = command.RecipientId;
        RentalId = command.RentalId;
        Amount = command.Amount;
        Currency = command.Currency ?? "PEN";
        Method = command.Method;
        Type = command.Type;
        Description = command.Description;
        DueDate = command.DueDate;
    }

    public void Update(UpdatePaymentCommand command)
    {
        PayerId = command.PayerId;
        RecipientId = command.RecipientId;
        RentalId = command.RentalId;
        Amount = command.Amount;
        Currency = command.Currency;
        Method = command.Method;
        Status = command.Status;
        TransactionId = command.TransactionId;
        Type = command.Type;
        Description = command.Description;
        Reason = command.Reason;
        DueDate = command.DueDate;
        CompletedAt = command.CompletedAt;
    }

    public void PartialUpdate(PatchPaymentCommand command)
    {
        if (!string.IsNullOrEmpty(command.Status))
            Status = command.Status;
        if (!string.IsNullOrEmpty(command.TransactionId))
            TransactionId = command.TransactionId;
        if (!string.IsNullOrEmpty(command.Reason))
            Reason = command.Reason;
        if (command.CompletedAt.HasValue)
            CompletedAt = command.CompletedAt;
    }

    public void Complete(string? transactionId)
    {
        Status = "completed";
        TransactionId = transactionId;
        CompletedAt = DateTime.UtcNow;
    }

    public void Fail(string? reason)
    {
        Status = "failed";
        Reason = reason;
    }

    public void Refund(string? reason)
    {
        Status = "refunded";
        Reason = reason;
    }
}
