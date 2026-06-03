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
}

public class ChangePasswordRequest
{
    public int UserId { get; set; }
    public string CurrentPassword { get; set; } = string.Empty;
    public string NewPassword { get; set; } = string.Empty;
}
