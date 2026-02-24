using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Exceptions;
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

    public async Task<List<TodoItem>> GetAllWithRelationsAsync(CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.TodoItemTags)
                .ThenInclude(tt => tt.Tag)
            .ToListAsync(cancellationToken);

    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default)
        => await _context.TodoItems.FindAsync([id], cancellationToken);

    public async Task<TodoItem?> GetByIdWithRelationsAsync(int id, CancellationToken cancellationToken = default)
        => await _context.TodoItems
            .Include(t => t.Category)
            .Include(t => t.TodoItemTags)
                .ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

    public async Task<TodoItem> CreateAsync(TodoItem todo, CancellationToken cancellationToken = default)
    {
        _context.TodoItems.Add(todo);
        await _context.SaveChangesAsync(cancellationToken);
        return todo;
    }

    public async Task<TodoItem?> UpdateAsync(int id, TodoItem todo, CancellationToken cancellationToken = default)
    {
        var existing = await _context.TodoItems
            .Include(t => t.TodoItemTags)
            .FirstOrDefaultAsync(t => t.Id == id, cancellationToken);

        if (existing is null) return null;

        // Appliquer le ConcurrencyStamp reçu du client pour la détection de conflit.
        // EF Core inclura ce stamp dans la clause WHERE du UPDATE :
        //   UPDATE TodoItems SET ... WHERE Id = @id AND ConcurrencyStamp = @expected
        // Si un autre utilisateur a modifié la ligne entre-temps, le WHERE ne matche pas
        // → 0 rows affected → DbUpdateConcurrencyException
        if (!string.IsNullOrEmpty(todo.ConcurrencyStamp))
        {
            _context.Entry(existing).Property(e => e.ConcurrencyStamp).OriginalValue = todo.ConcurrencyStamp;
        }

        existing.Title = todo.Title;
        existing.IsCompleted = todo.IsCompleted;
        existing.CategoryId = todo.CategoryId;

        // Générer un nouveau ConcurrencyStamp pour cette modification
        existing.ConcurrencyStamp = Guid.NewGuid().ToString();

        existing.TodoItemTags.Clear();
        existing.TodoItemTags = todo.TodoItemTags;

        try
        {
            await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException)
        {
            // Conflit détecté : l'entité a été modifiée par un autre utilisateur
            // On laisse la couche Application/API décider quoi faire (409 Conflict)
            throw new ConcurrencyConflictException(nameof(TodoItem), id);
        }

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
