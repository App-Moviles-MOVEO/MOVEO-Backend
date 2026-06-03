namespace Moveo_backend.IAM.Interfaces.REST.Resources;

public class AuthenticatedUserResource
{
    public int Id { get; set; }
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public string Dni { get; set; } = string.Empty;
    public string LicenseNumber { get; set; } = string.Empty;
    public string Role { get; set; } = "renter";
    public string Address { get; set; } = string.Empty;
}
