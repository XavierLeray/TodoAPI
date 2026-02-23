using TodoAPI.Domain.Entities;

namespace TodoAPI.Domain.Ports;

public interface IUserRepository
{
    Task<User?> GetByUsernameAsync(string username, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
    Task AddUserRoleAsync(int userId, int roleId, CancellationToken cancellationToken = default);
    Task<List<string>> GetUserRolesAsync(int userId, CancellationToken cancellationToken = default);
}