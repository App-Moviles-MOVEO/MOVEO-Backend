using Moveo_backend.UserManagement.Domain.Model.ValueObjects;

namespace Moveo_backend.IAM.Interfaces.REST.Resources;

public class LoginRequest
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class RegisterRequest
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string? Dni { get; set; }
    public string? LicenseNumber { get; set; }
    public string? Address { get; set; }
    public string Role { get; set; } = "renter";
    public UserPreferences? Preferences { get; set; }
    public string? Gender { get; set; }
}

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

public class ForgotPasswordRequest
{
    public string Email { get; set; } = string.Empty;
}

public class ResetPasswordRequest
{
    public string Token { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}

/// <summary>Cuerpo para que un admin resuelva una solicitud KYC.</summary>
public class ReviewKycRequest
{
    public bool Approve { get; set; }
    public string? RejectionReason { get; set; }
}

/// <summary>Item de la cola de revisión KYC (US12/US33).</summary>
public class KycReviewItemResource
{
    public int UserId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string? DniFrontUrl { get; set; }
    public string? DniBackUrl { get; set; }
    public string? SelfieUrl { get; set; }
    public DateTime? SubmittedAt { get; set; }
}
