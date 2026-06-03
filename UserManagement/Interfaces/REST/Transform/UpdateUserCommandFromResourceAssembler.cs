using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Model.ValueObjects;
using Moveo_backend.UserManagement.Interfaces.REST.Resources;

namespace Moveo_backend.UserManagement.Interfaces.REST.Transform;

public static class UpdateUserCommandFromResourceAssembler
{
    public static UpdateUserCommand ToCommandFromResource(UpdateUserResource resource, int userId)
    {
        var preferencesDto = resource.Preferences != null
            ? new UserPreferencesDto(
                resource.Preferences.Language,
                resource.Preferences.Notifications?.Email ?? true,
                resource.Preferences.Notifications?.Push ?? true,
                resource.Preferences.Notifications?.Sms ?? false,
                resource.Preferences.AutoAcceptRentals,
                resource.Preferences.MinimumRentalDays,
                resource.Preferences.InstantBooking
              )
            : null;

        return new UpdateUserCommand(
            userId,
            resource.FirstName,
            resource.LastName,
            resource.Phone,
            resource.Avatar,
            resource.BankAccount != null ? new BankAccountDto(
                resource.BankAccount.BankName,
                resource.BankAccount.AccountType,
                resource.BankAccount.AccountNumber,
                resource.BankAccount.Verified
            ) : null,
            preferencesDto
        );
    }
}

public record BankAccountDto(string BankName, string AccountType, string AccountNumber, bool Verified);
