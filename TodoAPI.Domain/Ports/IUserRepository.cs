using TodoAPI.Domain.Entities;

namespace TodoAPI.Domain.Ports;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task<bool> UsernameExistsAsync(string username, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, CancellationToken cancellationToken = default);
    Task AssignRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
}