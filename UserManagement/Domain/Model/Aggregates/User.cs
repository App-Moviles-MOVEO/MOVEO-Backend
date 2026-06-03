using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Model.ValueObjects;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Moveo_backend.UserManagement.Domain.Model.Aggregates;

public partial class User
{
    [Key]
    public int Id { get; private set; }

    // Columnas mapeadas directamente para EF
    public string FirstName { get; private set; } = string.Empty;
    public string LastName { get; private set; } = string.Empty;
    public string Email { get; private set; } = string.Empty;
    public string PasswordHash { get; private set; } = string.Empty;
    public string Role { get; private set; } = "renter";
    public string Phone { get; private set; } = string.Empty;
    public string Dni { get; private set; } = string.Empty;
    public string LicenseNumber { get; private set; } = string.Empty;
    public string Address { get; private set; } = string.Empty;
    public string? Avatar { get; private set; }
    
    // Verificación
    public bool EmailVerified { get; private set; }
    public bool PhoneVerified { get; private set; }
    public bool DniVerified { get; private set; }
    public bool LicenseVerified { get; private set; }
    
    // Estadísticas
    public int TotalRentals { get; private set; }
    public decimal TotalSpent { get; private set; }
    public decimal TotalEarned { get; private set; }
    public int ActiveRentals { get; private set; }
    public int CompletedRentals { get; private set; }
    public int CanceledRentals { get; private set; }
    
    // Datos bancarios
    public string? BankName { get; private set; }
    public string? BankAccountType { get; private set; }
    public string? BankAccountNumber { get; private set; }
    public bool BankAccountVerified { get; private set; }
    
    // Preferencias como columnas separadas
    public string PreferredLanguage { get; private set; } = "es";
    public bool EmailNotifications { get; private set; } = true;
    public bool PushNotifications { get; private set; } = true;
    public bool SmsNotifications { get; private set; } = false;
    public bool AutoAcceptRentals { get; private set; } = false;
    public int MinimumRentalDays { get; private set; } = 1;
    public bool InstantBooking { get; private set; } = false;

    public DateTime CreatedAt { get; private set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; private set; } = DateTime.UtcNow;
    
    // Propiedades de navegación (Value Objects como helpers, no mapeados)
    [NotMapped]
    public PersonName Name => new PersonName(FirstName, LastName);
    
    [NotMapped]
    public UserPreferences Preferences => new UserPreferences(
        PreferredLanguage, EmailNotifications, PushNotifications, 
        SmsNotifications, AutoAcceptRentals, MinimumRentalDays, InstantBooking);

    public string FullName => $"{FirstName} {LastName}";
    public string EmailAddress => Email;
    public string RoleName => Role;
    
    // Constructor para EF
    protected User() { }
    
    // Constructor desde CreateUserCommand
    public User(CreateUserCommand command)
    {
        FirstName = command.FirstName;
        LastName = command.LastName;
        Email = command.Email;
        PasswordHash = command.Password; // En producción debería hashearse
        Role = command.Role;
        Phone = command.Phone;
        Dni = command.Dni;
        LicenseNumber = command.LicenseNumber;
        Address = string.Empty;
        Avatar = command.Avatar;
        
        // Verificación
        EmailVerified = command.VerifiedEmail;
        PhoneVerified = command.VerifiedPhone;
        DniVerified = command.VerifiedDni;
        LicenseVerified = command.VerifiedLicense;
        
        // Estadísticas
        TotalRentals = command.TotalRentals;
        TotalSpent = command.TotalSpent;
        TotalEarned = command.TotalEarned;
        ActiveRentals = command.ActiveRentals;
        CompletedRentals = command.CompletedRentals;
        CanceledRentals = command.CanceledRentals;
        
        // Preferencias
        PreferredLanguage = command.Language;
        EmailNotifications = command.NotificationsEmail;
        PushNotifications = command.NotificationsPush;
        SmsNotifications = command.NotificationsSms;
        AutoAcceptRentals = false;
        MinimumRentalDays = 1;
        InstantBooking = false;
        
        // Datos bancarios
        BankName = command.BankName;
        BankAccountType = command.AccountType;
        BankAccountNumber = command.AccountNumber;
        BankAccountVerified = command.BankAccountVerified;
        
        CreatedAt = DateTime.UtcNow;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void Update(
        string firstName,
        string lastName,
        string phone,
        string address,
        UserPreferences preferences)
    {
        FirstName = firstName;
        LastName = lastName;
        Phone = phone;
        Address = address;
        PreferredLanguage = preferences.Language;
        EmailNotifications = preferences.EmailNotifications;
        PushNotifications = preferences.PushNotifications;
        SmsNotifications = preferences.SmsNotifications;
        AutoAcceptRentals = preferences.AutoAcceptRentals;
        MinimumRentalDays = preferences.MinimumRentalDays;
        InstantBooking = preferences.InstantBooking;
        UpdatedAt = DateTime.UtcNow;
    }

    public void ChangePassword(string newPassword)
    {
        PasswordHash = newPassword;
        UpdatedAt = DateTime.UtcNow;
    }

    public void UpdatePreferences(UserPreferences newPreferences)
    {
        PreferredLanguage = newPreferences.Language;
        EmailNotifications = newPreferences.EmailNotifications;
        PushNotifications = newPreferences.PushNotifications;
        SmsNotifications = newPreferences.SmsNotifications;
        AutoAcceptRentals = newPreferences.AutoAcceptRentals;
        MinimumRentalDays = newPreferences.MinimumRentalDays;
        InstantBooking = newPreferences.InstantBooking;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateAvatar(string avatarUrl)
    {
        Avatar = avatarUrl;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void UpdateBankAccount(string bankName, string accountType, string accountNumber, bool verified)
    {
        BankName = bankName;
        BankAccountType = accountType;
        BankAccountNumber = accountNumber;
        BankAccountVerified = verified;
        UpdatedAt = DateTime.UtcNow;
    }
    
    public void VerifyEmail() { EmailVerified = true; UpdatedAt = DateTime.UtcNow; }
    public void VerifyPhone() { PhoneVerified = true; UpdatedAt = DateTime.UtcNow; }
    public void VerifyDni() { DniVerified = true; UpdatedAt = DateTime.UtcNow; }
    public void VerifyLicense() { LicenseVerified = true; UpdatedAt = DateTime.UtcNow; }
    
    public void IncrementActiveRentals() { ActiveRentals++; TotalRentals++; UpdatedAt = DateTime.UtcNow; }
    public void CompleteRental(decimal amount, bool isOwner) 
    { 
        ActiveRentals--; 
        CompletedRentals++; 
        if (isOwner) TotalEarned += amount;
        else TotalSpent += amount;
        UpdatedAt = DateTime.UtcNow; 
    }
    public void CancelRental() { ActiveRentals--; CanceledRentals++; UpdatedAt = DateTime.UtcNow; }
}
