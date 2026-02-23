using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.CreateTodo;

public class CreateTodoCommandHandler : IRequestHandler<CreateTodoCommand, TodoItemResponse>
{
    private readonly ITodoRepository _repository;

    public CreateTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<TodoItemResponse> Handle(CreateTodoCommand request, CancellationToken cancellationToken)
    {
        var todo = TodoItem.Create(request.Title);
        var created = await _repository.CreateAsync(todo, cancellationToken);

        return new TodoItemResponse
        {
            Id = created.Id,
            Title = created.Title,
            IsCompleted = created.IsCompleted,
            CreatedAt = created.CreatedAt
        };
    }
}