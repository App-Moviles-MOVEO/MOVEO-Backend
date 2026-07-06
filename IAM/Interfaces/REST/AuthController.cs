using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using Moveo_backend.IAM.Domain.Model.Commands;
using Moveo_backend.IAM.Domain.Services;
using Moveo_backend.IAM.Interfaces.REST.Resources;

namespace Moveo_backend.IAM.Interfaces.REST;

[ApiController]
[Route("api/v1/auth")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;
    private readonly IWebHostEnvironment _environment;

    public AuthController(IAuthService authService, IWebHostEnvironment environment)
    {
        _authService = authService;
        _environment = environment;
    }

    /// <summary>
    /// Authenticate user and return profile data
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email and password are required" });

        var command = new LoginCommand(request.Email, request.Password);
        var user = await _authService.LoginAsync(command);

        if (user == null)
            return Unauthorized(new { message = "Invalid email or password" });

        return Ok(user);
    }

    /// <summary>
    /// Register a new user account
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        if (string.IsNullOrEmpty(request.Email) || string.IsNullOrEmpty(request.Password))
            return BadRequest(new { message = "Email and password are required" });

        if (string.IsNullOrEmpty(request.FirstName) || string.IsNullOrEmpty(request.LastName))
            return BadRequest(new { message = "First name and last name are required" });

        var command = new RegisterCommand(
            request.FirstName,
            request.LastName,
            request.Email,
            request.Password,
            request.Phone,
            request.Dni,
            request.LicenseNumber,
            request.Address,
            request.Role,
            request.Preferences,
            request.Gender
        );

        var user = await _authService.RegisterAsync(command);

        if (user == null)
            return Conflict(new { message = "User with this email already exists" });

        return CreatedAtAction(nameof(GetCurrentUser), new { userId = user.Id }, user);
    }

    /// <summary>
    /// Logout (no auth required)
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(typeof(object), StatusCodes.Status200OK)]
    public IActionResult Logout()
    {
        return Ok(new { message = "Logged out successfully" });
    }

    /// <summary>
    /// Get user info by id
    /// </summary>
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentUser([FromQuery] int userId)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId is required" });

        var user = await _authService.GetCurrentUserAsync(userId);
        if (user == null)
            return NotFound();

        return Ok(user);
    }

    /// <summary>
    /// Change password for user id
    /// </summary>
    [HttpPost("change-password")]
    public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        if (request.UserId <= 0)
            return BadRequest(new { message = "userId is required" });

        if (string.IsNullOrEmpty(request.CurrentPassword) || string.IsNullOrEmpty(request.NewPassword))
            return BadRequest(new { message = "Current password and new password are required" });

        var command = new AuthChangePasswordCommand(request.CurrentPassword, request.NewPassword);
        var success = await _authService.ChangePasswordAsync(request.UserId, command);

        if (!success)
            return BadRequest(new { message = "Invalid current password" });

        return Ok(new { message = "Password changed successfully" });
    }

    /// <summary>
    /// Request a password reset. Always returns 200 to avoid revealing whether the email exists.
    /// In non-production environments the one-time token is returned in the response for testing.
    /// </summary>
    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "email is required" });

        var devToken = await _authService.ForgotPasswordAsync(new ForgotPasswordCommand(request.Email));

        // Respuesta constante (200) tanto si el email existe como si no.
        var response = new Dictionary<string, object>
        {
            ["message"] = "If an account with that email exists, a reset link has been sent"
        };
        if (devToken != null)
            response["resetToken"] = devToken; // solo en dev, para probar sin servidor de correo

        return Ok(response);
    }

    /// <summary>
    /// Confirm a password reset using the one-time token.
    /// </summary>
    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Token) || string.IsNullOrWhiteSpace(request.NewPassword))
            return BadRequest(new { message = "token and newPassword are required" });

        var success = await _authService.ResetPasswordAsync(
            new ResetPasswordCommand(request.Token, request.NewPassword));

        if (!success)
            return BadRequest(new { message = "Invalid or expired token" });

        return Ok(new { message = "Password reset successfully" });
    }

    /// <summary>
    /// Upload KYC identity documents (multipart/form-data). Sets the user's KYC status to "pending".
    /// </summary>
    [HttpPost("kyc")]
    [Consumes("multipart/form-data")]
    [RequestSizeLimit(20_000_000)] // ~20 MB en total
    public async Task<IActionResult> UploadKyc(
        [FromForm] int userId,
        IFormFile? dniFront,
        IFormFile? dniBack,
        IFormFile? selfie)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId is required" });

        if (dniFront == null && dniBack == null && selfie == null)
            return BadRequest(new { message = "At least one document (dniFront, dniBack, selfie) is required" });

        var dniFrontUrl = await SaveKycFileAsync(userId, "dni_front", dniFront);
        var dniBackUrl = await SaveKycFileAsync(userId, "dni_back", dniBack);
        var selfieUrl = await SaveKycFileAsync(userId, "selfie", selfie);

        var status = await _authService.SubmitKycAsync(userId, dniFrontUrl, dniBackUrl, selfieUrl);
        if (status == null)
            return NotFound(new { message = "User not found" });

        return Ok(new { status });
    }

    /// <summary>
    /// Cola de revisión KYC: lista las solicitudes en estado "pending" (panel admin).
    /// </summary>
    [HttpGet("kyc/pending")]
    public async Task<IActionResult> GetPendingKyc()
    {
        var items = await _authService.GetPendingKycAsync();
        return Ok(items);
    }

    /// <summary>
    /// Resuelve una solicitud KYC (admin): approve o reject con motivo.
    /// </summary>
    [HttpPost("kyc/{userId:int}/review")]
    public async Task<IActionResult> ReviewKyc(int userId, [FromBody] ReviewKycRequest request)
    {
        if (userId <= 0)
            return BadRequest(new { message = "userId is required" });
        if (!request.Approve && string.IsNullOrWhiteSpace(request.RejectionReason))
            return BadRequest(new { message = "rejectionReason es obligatorio al rechazar" });

        var status = await _authService.ReviewKycAsync(userId, request.Approve, request.RejectionReason);
        if (status == null)
            return NotFound(new { message = "User not found" });

        return Ok(new { userId, status, rejectionReason = request.Approve ? null : request.RejectionReason });
    }

    // Guarda un archivo KYC en wwwroot/uploads/kyc/{userId}/ y devuelve su URL relativa.
    private async Task<string?> SaveKycFileAsync(int userId, string prefix, IFormFile? file)
    {
        if (file == null || file.Length == 0)
            return null;

        var webRoot = _environment.WebRootPath
                      ?? Path.Combine(_environment.ContentRootPath, "wwwroot");
        var folder = Path.Combine(webRoot, "uploads", "kyc", userId.ToString());
        Directory.CreateDirectory(folder);

        var ext = Path.GetExtension(file.FileName);
        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
        var fileName = $"{prefix}_{Guid.NewGuid():N}{ext}";
        var fullPath = Path.Combine(folder, fileName);

        await using (var stream = System.IO.File.Create(fullPath))
        {
            await file.CopyToAsync(stream);
        }

        return $"/uploads/kyc/{userId}/{fileName}";
    }
}
