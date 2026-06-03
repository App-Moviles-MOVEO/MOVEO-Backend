using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Model.Commands;
using Moveo_backend.UserManagement.Domain.Repositories;
using Moveo_backend.UserManagement.Domain.Services;
using Moveo_backend.UserManagement.Domain.Model.ValueObjects;

namespace Moveo_backend.UserManagement.Application.CommandServices;

public class UserCommandService : IUserCommandService
{
    private readonly IUserRepository _userRepository;

    public UserCommandService(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }

    public async Task<User> HandleCreate(CreateUserCommand command)
    {
        // Validación: no permitir usuarios duplicados por email
        if (await _userRepository.ExistsByEmailAsync(command.Email))
            throw new InvalidOperationException($"A user with email '{command.Email}' already exists.");

        // Crear la entidad User directamente desde el command
        var user = new User(command);

        await _userRepository.AddAsync(user);
        await _userRepository.SaveChangesAsync();

        return user;
    }

    public async Task<User> HandleUpdate(UpdateUserCommand command)
    {
        var existingUser = await _userRepository.FindByIdAsync(command.Id)
                           ?? throw new KeyNotFoundException($"User with ID {command.Id} not found.");

        // Solo actualizar campos que se proporcionan (PATCH)
        var updatedPreferences = command.Preferences != null 
            ? new UserPreferences(
                command.Preferences.Language,
                command.Preferences.EmailNotifications,
                command.Preferences.PushNotifications,
                command.Preferences.SmsNotifications,
                command.Preferences.AutoAcceptRentals,
                command.Preferences.MinimumRentalDays,
                command.Preferences.InstantBooking
            )
            : existingUser.Preferences;

        existingUser.Update(
            command.FirstName ?? existingUser.Name.FirstName,
            command.LastName ?? existingUser.Name.LastName,
            command.Phone ?? existingUser.Phone,
            existingUser.Address,
            updatedPreferences
        );

        _userRepository.Update(existingUser);
        await _userRepository.SaveChangesAsync();

        return existingUser;
    }

    public async Task HandleDelete(DeleteUserCommand command)
    {
        var user = await _userRepository.FindByIdAsync(command.UserId)
                   ?? throw new KeyNotFoundException($"User with ID {command.UserId} not found.");

        _userRepository.Remove(user);
        await _userRepository.SaveChangesAsync();
    }
}
