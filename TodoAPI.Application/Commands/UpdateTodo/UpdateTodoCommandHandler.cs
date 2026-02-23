using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.UpdateTodo;

public class UpdateTodoCommandHandler : IRequestHandler<UpdateTodoCommand, TodoItemResponse?>
{
    private readonly ITodoRepository _repository;

    public UpdateTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
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

        return new TodoItemResponse
        {
            Id = updated.Id,
            Title = updated.Title,
            IsCompleted = updated.IsCompleted,
            CreatedAt = updated.CreatedAt
        };
    }
}