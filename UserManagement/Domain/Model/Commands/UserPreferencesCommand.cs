namespace Moveo_backend.UserManagement.Domain.Model.Commands;

public record UserPreferencesCommand(
    string Language,
    bool EmailNotifications,
    bool PushNotifications,
    bool SmsNotifications,
    bool AutoAcceptRentals,
    int MinimumRentalDays,
    bool InstantBooking
);