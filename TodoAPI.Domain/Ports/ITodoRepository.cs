using TodoAPI.Domain.Entities;

namespace TodoAPI.Domain.Ports;

public interface ITodoRepository
{
    Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default);
    Task<List<TodoItem>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItem?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default);
    Task<TodoItem> CreateAsync(TodoItem todo, CancellationToken cancellationToken = default);
    Task<TodoItem?> UpdateAsync(int id, TodoItem todo, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default);
    Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default);
}