namespace Moveo_backend.UserManagement.Domain.Model.ValueObjects;

public record Password(string Value)
{
    public bool IsSecure() => Value.Length >= 6;

    public Password() : this(string.Empty)
    {
    }
}