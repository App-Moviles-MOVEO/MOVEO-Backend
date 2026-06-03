namespace Moveo_backend.UserManagement.Domain.Model.ValueObjects;

public record EmailAddress(string Address)
{
    public bool IsValid() => !string.IsNullOrWhiteSpace(Address) && Address.Contains("@");
    public EmailAddress() : this(string.Empty)
    {
    }

}