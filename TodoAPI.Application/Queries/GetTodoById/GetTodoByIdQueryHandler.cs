using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Queries.GetTodoById;

public class GetTodoByIdQueryHandler : IRequestHandler<GetTodoByIdQuery, TodoItemResponse?>
{
    private readonly ITodoRepository _repository;

    public GetTodoByIdQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemResponse?> Handle(GetTodoByIdQuery request, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdWithRelationsAsync(request.Id, cancellationToken);
        if (todo is null) return null;

        return new TodoItemResponse
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
}