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
            IsCompleted = request.IsCompleted
        };

        var updated = await _repository.UpdateAsync(request.Id, todo, cancellationToken);
        if (updated is null) return null;

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        return new TodoItemResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            IsCompleted = updated.IsCompleted,
            CreatedAt = updated.CreatedAt
        };
    }
}