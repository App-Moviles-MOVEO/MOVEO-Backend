using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Interfaces.REST.Resources;

namespace Moveo_backend.UserManagement.Interfaces.REST.Transform;

public static class CreateUserCommandFromResourceAssembler
{
    public static CreateUserCommand ToCommandFromResource(CreateUserResource resource)
    {
        return new CreateUserCommand(
            resource.FirstName,
            resource.LastName,
            resource.Email,
            resource.Password,
            resource.Phone ?? string.Empty,
            resource.Dni ?? string.Empty,
            resource.LicenseNumber ?? string.Empty,
            resource.Role,
            resource.Avatar,
            resource.Verified?.Email ?? false,
            resource.Verified?.Phone ?? false,
            resource.Verified?.Dni ?? false,
            resource.Verified?.License ?? false,
            resource.Stats?.TotalRentals ?? 0,
            resource.Stats?.TotalSpent ?? 0,
            resource.Stats?.TotalEarned ?? 0,
            resource.Stats?.ActiveRentals ?? 0,
            resource.Stats?.CompletedRentals ?? 0,
            resource.Stats?.CanceledRentals ?? 0,
            resource.Preferences?.Language ?? "es",
            resource.Preferences?.Notifications?.Email ?? true,
            resource.Preferences?.Notifications?.Push ?? true,
            resource.Preferences?.Notifications?.Sms ?? false,
            resource.BankAccount?.BankName,
            resource.BankAccount?.AccountType,
            resource.BankAccount?.AccountNumber,
            resource.BankAccount?.Verified ?? false
        );
    }
}