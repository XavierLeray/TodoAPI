using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Queries.GetAllTodos;

public class GetAllTodosQueryHandler : IRequestHandler<GetAllTodosQuery, List<TodoItemResponse>>
{
    private readonly ITodoRepository _repository;

    public GetAllTodosQueryHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<List<TodoItemResponse>> Handle(GetAllTodosQuery request, CancellationToken cancellationToken)
    {
        var todos = await _repository.GetAllAsync(cancellationToken);

        return todos.Select(t => new TodoItemResponse
        {
            Id = t.Id,
            Title = t.Title,
            IsCompleted = t.IsCompleted,
            CreatedAt = t.CreatedAt
        }).ToList();
    }
}