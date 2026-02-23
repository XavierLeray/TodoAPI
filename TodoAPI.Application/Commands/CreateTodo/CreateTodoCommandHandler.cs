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
        var todo = TodoItem.Create(request.Title);
        var created = await _repository.CreateAsync(todo, cancellationToken);

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        return new TodoItemResponse
        {
            Id = created.Id,
            Title = created.Title,
            IsCompleted = created.IsCompleted,
            CreatedAt = created.CreatedAt
        };
    }
}