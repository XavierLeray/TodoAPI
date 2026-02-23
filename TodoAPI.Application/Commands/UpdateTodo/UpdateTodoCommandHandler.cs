using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.UpdateTodo;

public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, TodoItemResponse?>
{
    private readonly ITodoRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "todos:all";

    public UpdateTodoCommandHandler(ITodoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<TodoItemResponse?> Handle(UpdateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new TodoItem
        {
            Id = request.Id,
            Title = request.Title,
            IsCompleted = request.IsCompleted,
            CategoryId = request.CategoryId,
            TodoItemTags = request.TagIds.Select(tagId => new TodoItemTag
            {
                TagId = tagId,
                AssignedAt = DateTime.UtcNow
            }).ToList()
        };

        var updated = await _repository.UpdateAsync(request.Id, todo, cancellationToken);
        if (updated is null) return null;

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        var todoWithRelations = await _repository.GetByIdWithRelationsAsync(updated.Id, cancellationToken);

        return MapToResponse(todoWithRelations!);
    }

    private static TodoItemResponse MapToResponse(TodoItem todo) => new()
    {
        Id = todo.Id,
        Title = todo.Title,
        IsCompleted = todo.IsCompleted,
        CreatedAt = todo.CreatedAt,
        Category = todo.Category != null ? new CategoryDto
        {
            Id = todo.Category.Id,
            Name = todo.Category.Name,
            Color = todo.Category.Color
        } : null,
        Tags = todo.TodoItemTags.Select(tt => new TagDto
        {
            Id = tt.Tag.Id,
            Name = tt.Tag.Name
        }).ToList()
    };
}