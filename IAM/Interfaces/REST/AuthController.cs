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

    public AuthController(IAuthService authService)
    {
        _authService = authService;
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
            request.Preferences
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
}
