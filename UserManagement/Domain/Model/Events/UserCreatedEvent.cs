namespace Moveo_backend.UserManagement.Domain.Model.Events;

public record UserCreatedEvent(int UserId, string FullName, string Email, string Role);
