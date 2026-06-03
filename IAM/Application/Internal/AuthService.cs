using Microsoft.EntityFrameworkCore;
using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Domain.Services;
using Moveo_backend.IAM.Infrastructure.Hashing;
using Moveo_backend.IAM.Interfaces.REST.Resources;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;

namespace Moveo_backend.IAM.Application.Internal;

public class AuthService : IAuthService
{
    private readonly AppDbContext _context;
    private readonly IHashingService _hashingService;

    public AuthService(
        AppDbContext context,
        IHashingService hashingService)
    {
        _context = context;
        _hashingService = hashingService;
    }

    public async Task<AuthenticatedUserResource?> LoginAsync(LoginCommand command)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == command.Email.ToLower());

        if (user == null)
            return null;

        if (!_hashingService.VerifyPassword(command.Password, user.PasswordHash))
            return null;

        return MapToAuthenticatedUser(user);
    }

    public async Task<AuthenticatedUserResource?> RegisterAsync(RegisterCommand command)
    {
        // Check if user already exists
        var existingUser = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == command.Email.ToLower());

        if (existingUser != null)
            return null;

        var hashedPassword = _hashingService.HashPassword(command.Password);

        var user = new User(new CreateUserCommand(
            FirstName: command.FirstName,
            LastName: command.LastName,
            Email: command.Email,
            Password: hashedPassword,
            Phone: command.Phone ?? string.Empty,
            Dni: command.Dni ?? string.Empty,
            LicenseNumber: command.LicenseNumber ?? string.Empty,
            Role: command.Role
        ));

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        return MapToAuthenticatedUser(user);
    }

    public async Task<AuthenticatedUserResource?> GetCurrentUserAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        return MapToAuthenticatedUser(user);
    }

    public async Task<bool> ChangePasswordAsync(int userId, AuthChangePasswordCommand command)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return false;

        if (!_hashingService.VerifyPassword(command.CurrentPassword, user.PasswordHash))
            return false;

        user.ChangePassword(_hashingService.HashPassword(command.NewPassword));
        await _context.SaveChangesAsync();

        return true;
    }

    private static AuthenticatedUserResource MapToAuthenticatedUser(User user)
    {
        return new AuthenticatedUserResource
        {
            Id = user.Id,
            FirstName = user.Name.FirstName,
            LastName = user.Name.LastName,
            Email = user.EmailAddress,
            Phone = user.Phone,
            Dni = user.Dni,
            LicenseNumber = user.LicenseNumber,
            Role = user.RoleName,
            Address = user.Address
        };
    }
}
