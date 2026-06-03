namespace Moveo_backend.UserManagement.Interfaces.REST.Resources;

public class CreateUserResource
{
    public string Role { get; set; } = "renter";
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public UserVerificationResource? Verified { get; set; }
    public UserStatsResource? Stats { get; set; }
    public UserPreferencesResource? Preferences { get; set; }
    public BankAccountResource? BankAccount { get; set; }
}