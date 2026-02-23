using System.Collections.Concurrent;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Infrastructure.Repositories;

public class InMemoryTodoRepository : ITodoRepository
{
    private readonly ConcurrentDictionary<int, TodoItem> _todos = new();
    private int _nextId = 0;

    public Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => Task.FromResult(_todos.Values.ToList());

    public Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
    {
        _todos.TryGetValue(id, out var todo);
        return Task.FromResult(todo);
    }

    public Task<TodoItem> CreateAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _nextId);
        todo.Id = id;
        _todos.TryAdd(id, todo);
        return Task.FromResult(todo);
    }

    public Task<TodoItem?> UpdateAsync(int id, TodoItem todo, CancellationToken cancellationToken = default)
    {
        if (!_todos.TryGetValue(id, out var existing))
            return Task.FromResult<TodoItem?>(null);

        existing.Title = todo.Title;
        existing.IsCompleted = todo.IsCompleted;

        return Task.FromResult<TodoItem?>(existing);
    }

    public Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
        => Task.FromResult(_todos.TryRemove(id, out _));

    public Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default)
    {
        var exists = _todos.Values.Any(t =>
            t.Title.Equals(title, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult(exists);
    }
}