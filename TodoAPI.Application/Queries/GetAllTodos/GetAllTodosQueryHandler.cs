using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Queries.GetAllTodos;

public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, List<TodoItemResponse>>
{
    private readonly ITodoRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "todos:all";

    public GetAllTodosQueryHandler(ITodoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<TodoItemResponse>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<List<TodoItemResponse>>(CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var todos = await _repository.GetAllWithRelationsAsync(cancellationToken);

        var response = todos.Select(MapToResponse).ToList();

        await _cache.SetAsync(CacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
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