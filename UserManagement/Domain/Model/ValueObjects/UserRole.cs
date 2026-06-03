namespace Moveo_backend.UserManagement.Domain.Model.ValueObjects;

public record UserRole(string Value)
{
    public static readonly string Owner = "Owner";
    public static readonly string Renter = "Renter";

    public bool IsValidRole() => Value == Owner || Value == Renter;

    public UserRole() : this(Renter)
    {
    }
}