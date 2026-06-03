namespace Moveo_backend.UserManagement.Interfaces.REST.Resources;

public class LoginResource
{
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
}

public class LoginResponseResource
{
    public int Id { get; set; }
    public string Role { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string? Avatar { get; set; }
    public UserVerificationResource Verified { get; set; } = new();
    public UserStatsResource Stats { get; set; } = new();
    public UserPreferencesResource Preferences { get; set; } = new();
    public BankAccountResource? BankAccount { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public string Message { get; set; } = "Login successful";
}
