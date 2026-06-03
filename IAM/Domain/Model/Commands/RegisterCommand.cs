using Moveo_backend.UserManagement.Domain.Model.ValueObjects;

namespace Moveo_backend.IAM.Domain.Model.Commands;

public record RegisterCommand(
    string FirstName,
    string LastName,
    string Email,
    string Password,
    string? Phone = null,
    string? Dni = null,
    string? LicenseNumber = null,
    string? Address = null,
    string Role = "renter",
    UserPreferences? Preferences = null
);
