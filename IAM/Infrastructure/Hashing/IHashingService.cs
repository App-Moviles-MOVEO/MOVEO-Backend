namespace Moveo_backend.IAM.Infrastructure.Hashing;

public interface IHashingService
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
