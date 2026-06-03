using Moveo_backend.Support.Domain.Model.Commands;

namespace Moveo_backend.Support.Domain.Model.Aggregate;

public class SupportTicket
{
    public int Id { get; private set; }
    public int UserId { get; private set; }
    public string Subject { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; } // technical, billing, general, rental_issue, damage, etc.
    public string Status { get; private set; } // open, in_progress, resolved, closed
    public string Priority { get; private set; } // low, medium, high, urgent
    public string Type { get; private set; } // general, complaint, request, damage, etc.
    public int? RelatedId { get; private set; }
    public string? RelatedType { get; private set; } // rental, vehicle, payment, etc.
    public string? ResolutionNotes { get; private set; }
    public int? AssignedToId { get; private set; }
    public DateTime CreatedAt { get; private set; }
    public DateTime UpdatedAt { get; private set; }
    public DateTime? ResolvedAt { get; private set; }
    
    // Damage ticket fields
    public decimal? EstimatedCost { get; private set; }
    public int? VehicleId { get; private set; }
    public string? VehicleName { get; private set; }
    public int? RentalId { get; private set; }
    public int? RenterId { get; private set; }
    public string? RenterName { get; private set; }
    public string? AttachmentsJson { get; private set; } // JSON array of attachment URLs
    
    // Dispute fields
    public string? DisputeStatus { get; private set; } // null, disputed, owner_proof_sent, resolved_in_favor_owner, resolved_in_favor_renter
    public string? DisputeReason { get; private set; }
    public string? DisputeDescription { get; private set; }
    public DateTime? DisputedAt { get; private set; }
    public int? DisputedBy { get; private set; }
    
    // Additional proof fields (owner response to dispute)
    public string? AdditionalProofJson { get; private set; } // JSON array of proof URLs
    public string? AdditionalProofMessage { get; private set; }
    public DateTime? AdditionalProofSentAt { get; private set; }
    
    // Payment fields
    public string? PaymentStatus { get; private set; } // pending, paid, waived, disputed
    public DateTime? PaidAt { get; private set; }
    
    // Navigation property
    public ICollection<TicketMessage> Messages { get; private set; } = new List<TicketMessage>();

    protected SupportTicket() 
    {
        Subject = string.Empty;
        Description = string.Empty;
        Category = string.Empty;
        Status = string.Empty;
        Priority = string.Empty;
        Type = string.Empty;
    }

    public SupportTicket(CreateSupportTicketCommand command)
    {
        UserId = command.UserId;
        Subject = command.Subject;
        Description = command.Description;
        Category = command.Category;
        Status = "open";
        Priority = command.Priority ?? "medium";
        Type = command.Type ?? "general";
        RelatedId = command.RelatedId;
        RelatedType = command.RelatedType;
        
        // Damage ticket fields
        EstimatedCost = command.EstimatedCost;
        VehicleId = command.VehicleId;
        VehicleName = command.VehicleName;
        RentalId = command.RentalId;
        RenterId = command.RenterId;
        RenterName = command.RenterName;
        AttachmentsJson = command.AttachmentsJson;
        
        // Initialize payment status for damage tickets
        if (Type == "damage" && EstimatedCost.HasValue)
        {
            PaymentStatus = "pending";
        }
        
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void Update(UpdateSupportTicketCommand command)
    {
        if (command.Status is not null) Status = command.Status;
        if (command.Priority is not null) Priority = command.Priority;
        if (command.AssignedToId.HasValue) AssignedToId = command.AssignedToId.Value;
        if (command.ResolutionNotes is not null) ResolutionNotes = command.ResolutionNotes;
        if (command.EstimatedCost.HasValue) EstimatedCost = command.EstimatedCost;
        
        // Handle dispute flow
        if (command.DisputeStatus is not null)
        {
            DisputeStatus = command.DisputeStatus;
            if (command.DisputeStatus == "disputed" && DisputedAt is null)
            {
                DisputedAt = DateTime.UtcNow;
                DisputedBy = command.DisputedBy;
            }
        }
        if (command.DisputeReason is not null) DisputeReason = command.DisputeReason;
        if (command.DisputeDescription is not null) DisputeDescription = command.DisputeDescription;
        
        // Handle additional proof (owner response to dispute)
        if (command.AdditionalProofJson is not null)
        {
            AdditionalProofJson = command.AdditionalProofJson;
            AdditionalProofSentAt = DateTime.UtcNow;
            if (DisputeStatus == "disputed")
            {
                DisputeStatus = "owner_proof_sent";
            }
        }
        if (command.AdditionalProofMessage is not null) AdditionalProofMessage = command.AdditionalProofMessage;
        
        // Handle payment status
        if (command.PaymentStatus is not null)
        {
            PaymentStatus = command.PaymentStatus;
            if (command.PaymentStatus == "paid" && PaidAt is null)
            {
                PaidAt = DateTime.UtcNow;
            }
        }
        
        if (command.Status == "resolved" && ResolvedAt is null)
        {
            ResolvedAt = DateTime.UtcNow;
        }
        
        UpdatedAt = DateTime.UtcNow;
    }

    public void Close()
    {
        Status = "closed";
        if (ResolvedAt is null) ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }

    public void AddMessage(TicketMessage message)
    {
        Messages.Add(message);
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Method to initiate a dispute
    public void InitiateDispute(int disputedById, string reason, string? description)
    {
        if (Type != "damage")
            throw new InvalidOperationException("Only damage tickets can be disputed");
        
        if (DisputeStatus is not null)
            throw new InvalidOperationException("This ticket has already been disputed");
        
        DisputeStatus = "disputed";
        DisputeReason = reason;
        DisputeDescription = description;
        DisputedAt = DateTime.UtcNow;
        DisputedBy = disputedById;
        PaymentStatus = "disputed";
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Method for owner to send additional proof
    public void SendAdditionalProof(string proofJson, string? message)
    {
        if (DisputeStatus != "disputed")
            throw new InvalidOperationException("Additional proof can only be sent for disputed tickets");
        
        AdditionalProofJson = proofJson;
        AdditionalProofMessage = message;
        AdditionalProofSentAt = DateTime.UtcNow;
        DisputeStatus = "owner_proof_sent";
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Method to resolve dispute
    public void ResolveDispute(string resolution, string? notes)
    {
        if (DisputeStatus is null || DisputeStatus == "resolved_in_favor_owner" || DisputeStatus == "resolved_in_favor_renter")
            throw new InvalidOperationException("This dispute has already been resolved or was never disputed");
        
        DisputeStatus = resolution; // resolved_in_favor_owner or resolved_in_favor_renter
        ResolutionNotes = notes;
        
        if (resolution == "resolved_in_favor_renter")
        {
            PaymentStatus = "waived";
        }
        else
        {
            PaymentStatus = "pending"; // Renter must pay
        }
        
        UpdatedAt = DateTime.UtcNow;
    }
    
    // Method to mark as paid
    public void MarkAsPaid()
    {
        if (PaymentStatus == "paid")
            throw new InvalidOperationException("This ticket has already been paid");
        
        PaymentStatus = "paid";
        PaidAt = DateTime.UtcNow;
        Status = "resolved";
        ResolvedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
}
