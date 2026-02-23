using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;
using TodoAPI.Infrastructure.Data;

namespace TodoAPI.Infrastructure.Repositories;

public class EfTodoRepository : ITodoRepository
{
    private readonly AppDbContext _context;

    public EfTodoRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<List<TodoItem>> GetAllAsync(CancellationToken cancellationToken = default)
        => await _context.TodoItems.ToListAsync(cancellationToken);

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.TodoItems.FindAsync([id], cancellationToken);

    public async Task<TodoItem> CreateAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task<TodoItem?> UpdateAsync(int id, TodoItem todo, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TodoItems.FindAsync([id], cancellationToken);
        if (existing is null) return null;

        existing.Title = todo.Title;
        existing.IsCompleted = todo.IsCompleted;

        await _context.SaveChangesAsync(cancellationToken);
        return existing;
    }

    public async Task<bool> DeleteAsync(int id, CancellationToken cancellationToken = default)
    {
        var todo = await _context.TodoItems.FindAsync([id], cancellationToken);
        if (todo is null) return false;

        _context.TodoItems.Remove(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return true;
    }

    public async Task<bool> ExistsByTitleAsync(string title, CancellationToken cancellationToken = default)
        => await _context.TodoItems.AnyAsync(t => t.Title == title, cancellationToken);
}