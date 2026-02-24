using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.CreateTodo;

public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, TodoItemResponse>
{
    private readonly ITodoRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "todos:all";

    public CreateTodoCommandHandler(ITodoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<TodoItemResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = new TodoItem
        {
            Title = request.Title,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow,
            CategoryId = request.CategoryId,
            TodoItemTags = request.TagIds.Select(tagId => new TodoItemTag
            {
                TagId = tagId,
                AssignedAt = DateTime.UtcNow
            }).ToList()
        };

        var created = await _repository.CreateAsync(todo, cancellationToken);
        await _cache.RemoveAsync(CacheKey, cancellationToken);

        var todoWithRelations = await _repository.GetByIdWithRelationsAsync(created.Id, cancellationToken);

        return MapToResponse(todoWithRelations!);
    }

    private static TodoItemResponse MapToResponse(TodoItem todo) => new()
    {
        Id = todo.Id,
        Title = todo.Title,
        IsCompleted = todo.IsCompleted,
        CreatedAt = todo.CreatedAt,
        ConcurrencyStamp = todo.ConcurrencyStamp,
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