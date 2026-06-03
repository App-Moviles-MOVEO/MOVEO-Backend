namespace Moveo_backend.UserManagement.Interfaces.REST.Resources;

public class UserResource
{
    public int Id { get; set; }
    public string Role { get; set; } = "renter";
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
}

public class UserVerificationResource
{
    public bool Email { get; set; } = false;
    public bool Phone { get; set; } = false;
    public bool Dni { get; set; } = false;
    public bool License { get; set; } = false;
}

public class UserStatsResource
{
    public int TotalRentals { get; set; } = 0;
    public decimal TotalSpent { get; set; } = 0;
    public decimal TotalEarned { get; set; } = 0;
    public int ActiveRentals { get; set; } = 0;
    public int CompletedRentals { get; set; } = 0;
    public int CanceledRentals { get; set; } = 0;
}

public class UserPreferencesResource
{
    public string Language { get; set; } = "es";
    public NotificationPreferencesResource Notifications { get; set; } = new();
    
    // Propiedades de compatibilidad con el formato antiguo
    public bool EmailNotifications => Notifications.Email;
    public bool PushNotifications => Notifications.Push;
    public bool SmsNotifications => Notifications.Sms;
    public bool AutoAcceptRentals { get; set; } = false;
    public int MinimumRentalDays { get; set; } = 1;
    public bool InstantBooking { get; set; } = false;
}

public class NotificationPreferencesResource
{
    public bool Email { get; set; } = true;
    public bool Push { get; set; } = true;
    public bool Sms { get; set; } = false;
}

public class BankAccountResource
{
    public string BankName { get; set; } = string.Empty;
    public string AccountType { get; set; } = "ahorro";
    public string AccountNumber { get; set; } = string.Empty;
    public string Cci { get; set; } = string.Empty;
    public bool Verified { get; set; } = false;
}