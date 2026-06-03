namespace Moveo_backend.UserManagement.Domain.Model.ValueObjects;

public class UserPreferences
{
    public string Language { get; private set; }
    public bool EmailNotifications { get; private set; }
    public bool PushNotifications { get; private set; }
    public bool SmsNotifications { get; private set; }
    public bool AutoAcceptRentals { get; private set; }
    public int MinimumRentalDays { get; private set; }
    public bool InstantBooking { get; private set; }

    // Constructor completo
    public UserPreferences(
        string language,
        bool emailNotifications,
        bool pushNotifications,
        bool smsNotifications,
        bool autoAcceptRentals,
        int minimumRentalDays,
        bool instantBooking)
    {
        Language = language;
        EmailNotifications = emailNotifications;
        PushNotifications = pushNotifications;
        SmsNotifications = smsNotifications;
        AutoAcceptRentals = autoAcceptRentals;
        MinimumRentalDays = minimumRentalDays;
        InstantBooking = instantBooking;
    }

    // Método auxiliar opcional si luego quieres modificarlo de forma controlada
    public void Update(
        string? language = null,
        bool? emailNotifications = null,
        bool? pushNotifications = null,
        bool? smsNotifications = null,
        bool? autoAcceptRentals = null,
        int? minimumRentalDays = null,
        bool? instantBooking = null)
    {
        Language = language ?? Language;
        EmailNotifications = emailNotifications ?? EmailNotifications;
        PushNotifications = pushNotifications ?? PushNotifications;
        SmsNotifications = smsNotifications ?? SmsNotifications;
        AutoAcceptRentals = autoAcceptRentals ?? AutoAcceptRentals;
        MinimumRentalDays = minimumRentalDays ?? MinimumRentalDays;
        InstantBooking = instantBooking ?? InstantBooking;
    }
}