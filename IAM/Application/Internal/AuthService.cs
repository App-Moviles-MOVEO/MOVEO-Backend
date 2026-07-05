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
    private readonly IWebHostEnvironment _environment;

    public AuthService(
        AppDbContext context,
        IHashingService hashingService,
        IWebHostEnvironment environment)
    {
        _context = context;
        _hashingService = hashingService;
        _environment = environment;
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

    public async Task<string?> ForgotPasswordAsync(ForgotPasswordCommand command)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email.ToLower() == command.Email.ToLower());

        // No revelar si el email existe: si no hay usuario, no generamos token.
        if (user == null)
            return null;

        var token = Guid.NewGuid().ToString("N");
        user.SetPasswordResetToken(token, DateTime.UtcNow.AddMinutes(30));
        await _context.SaveChangesAsync();

        // TODO: enviar el token por correo. Mientras no haya servidor de mail,
        // en desarrollo lo devolvemos para poder probar el flujo end-to-end.
        return _environment.IsProduction() ? null : token;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordCommand command)
    {
        if (string.IsNullOrWhiteSpace(command.Token) || string.IsNullOrWhiteSpace(command.NewPassword))
            return false;

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.PasswordResetToken == command.Token);

        if (user == null || !user.IsPasswordResetTokenValid(command.Token))
            return false;

        user.ResetPassword(_hashingService.HashPassword(command.NewPassword));
        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<string?> SubmitKycAsync(int userId, string? dniFrontUrl, string? dniBackUrl, string? selfieUrl)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        user.SubmitKyc(dniFrontUrl, dniBackUrl, selfieUrl);
        await _context.SaveChangesAsync();

        return user.KycStatus;
    }

    public async Task<IEnumerable<KycReviewItemResource>> GetPendingKycAsync()
    {
        return await _context.Users
            .Where(u => u.KycStatus == "pending")
            .OrderBy(u => u.KycSubmittedAt)
            .Select(u => new KycReviewItemResource
            {
                UserId = u.Id,
                FullName = u.FirstName + " " + u.LastName,
                Email = u.Email,
                Dni = u.Dni,
                DniFrontUrl = u.KycDniFrontUrl,
                DniBackUrl = u.KycDniBackUrl,
                SelfieUrl = u.KycSelfieUrl,
                SubmittedAt = u.KycSubmittedAt
            })
            .ToListAsync();
    }

    public async Task<string?> ReviewKycAsync(int userId, bool approve, string? rejectionReason)
    {
        var user = await _context.Users.FindAsync(userId);
        if (user == null)
            return null;

        if (approve) user.ApproveKyc();
        else user.RejectKyc(rejectionReason);

        await _context.SaveChangesAsync();
        return user.KycStatus;
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
            Address = user.Address,
            KycStatus = user.KycStatus
        };
    }
}
