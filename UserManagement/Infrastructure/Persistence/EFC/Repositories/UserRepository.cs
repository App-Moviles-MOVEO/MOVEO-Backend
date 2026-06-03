using Microsoft.EntityFrameworkCore;
using Moveo_backend.Shared.Infrastructure.Persistence.EFC.Configuration;
using Moveo_backend.UserManagement.Domain.Model.Aggregates;
using Moveo_backend.UserManagement.Domain.Repositories;

namespace Moveo_backend.UserManagement.Infrastructure.Persistence.EFC.Repositories;

public class UserRepository : IUserRepository
{
    private readonly AppDbContext _context;

    public UserRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<User>> GetAllAsync()
    {
        return await _context.Set<User>().ToListAsync();
    }

    public async Task<User?> FindByIdAsync(int id)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Id == id);
    }

    public async Task<User?> FindByEmailAsync(string email)
    {
        return await _context.Set<User>().FirstOrDefaultAsync(u => u.Email == email);
    }

    public async Task<User?> FindByRoleAsync(string role)
    {
        return await _context.Set<User>()
            .FirstOrDefaultAsync(u => u.Role.ToLower() == role.ToLower());
    }

    public async Task AddAsync(User user)
    {
        await _context.Set<User>().AddAsync(user);
    }

    public void Update(User user)
    {
        _context.Set<User>().Update(user);
    }

    public void Remove(User user)
    {
        _context.Set<User>().Remove(user);
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        return await _context.Set<User>().AnyAsync(u => u.Email == email);
    }

    public async Task SaveChangesAsync()
    {
        await _context.SaveChangesAsync();
    }
}