using Moveo_backend.UserManagement.Interfaces.REST.Transform;

namespace Moveo_backend.UserManagement.Domain.Model.Commands;

public record UpdateUserCommand(
    int Id,
    string? FirstName,
    string? LastName,
    string? Phone,
    string? Avatar,
    BankAccountDto? BankAccount,
    UserPreferencesDto? Preferences
);

public record UserPreferencesDto(
    string Language,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    bool AutoAcceptRentals,
    int MinimumRentalDays,
    bool InstantBooking
);