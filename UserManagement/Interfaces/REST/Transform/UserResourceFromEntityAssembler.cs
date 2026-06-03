using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Interfaces.REST.Resources;

namespace Moveo_backend.UserManagement.Interfaces.REST.Transform;

public static class UserResourceFromEntityAssembler
{
    public static UserResource ToResourceFromEntity(User user)
    {
        return new UserResource
        {
            Id = user.Id,
            Role = user.Role,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email,
            Phone = user.Phone,
            Dni = user.Dni,
            LicenseNumber = user.LicenseNumber,
            Avatar = user.Avatar,
            Verified = new UserVerificationResource
            {
                Email = user.EmailVerified,
                Phone = user.PhoneVerified,
                Dni = user.DniVerified,
                License = user.LicenseVerified
            },
            Stats = new UserStatsResource
            {
                TotalRentals = user.TotalRentals,
                TotalSpent = user.TotalSpent,
                TotalEarned = user.TotalEarned,
                ActiveRentals = user.ActiveRentals,
                CompletedRentals = user.CompletedRentals,
                CanceledRentals = user.CanceledRentals
            },
            Preferences = new UserPreferencesResource
            {
                Language = user.PreferredLanguage ?? "es",
                Notifications = new NotificationPreferencesResource
                {
                    Email = user.EmailNotifications,
                    Push = user.PushNotifications,
                    Sms = user.SmsNotifications
                }
            },
            BankAccount = user.BankAccountNumber != null ? new BankAccountResource
            {
                BankName = user.BankName ?? "",
                AccountType = user.BankAccountType ?? "",
                AccountNumber = user.BankAccountNumber,
                Verified = user.BankAccountVerified
            } : null,
            CreatedAt = user.CreatedAt,
            UpdatedAt = user.UpdatedAt
        };
    }
}