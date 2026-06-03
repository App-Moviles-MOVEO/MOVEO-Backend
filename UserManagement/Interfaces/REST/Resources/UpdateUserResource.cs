namespace Moveo_backend.UserManagement.Interfaces.REST.Resources;

public record UpdateUserResource(
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Avatar,
    BankAccountResource? BankAccount,
    UserPreferencesResource? Preferences
);