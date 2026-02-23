using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
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

        var todos = await _repository.GetAllAsync(cancellationToken);

        var response = todos.Select(t => new TodoItemResponse
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        }).ToList();

        await _cache.SetAsync(CacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
    }
}