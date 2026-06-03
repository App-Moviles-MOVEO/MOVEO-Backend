using Moveo_backend.UserManagement.Domain.Model.Aggregates;

namespace Moveo_backend.UserManagement.Domain.Repositories;

public interface IUserRepository
{
    Task<IEnumerable<User>> GetAllAsync();
    Task<User?> FindByIdAsync(int id);
    Task<User?> FindByEmailAsync(string email);
    Task<User?> FindByRoleAsync(string role);
    Task AddAsync(User user);
    void Update(User user);
    void Remove(User user);
    Task<bool> ExistsByEmailAsync(string email);
    Task SaveChangesAsync();
}