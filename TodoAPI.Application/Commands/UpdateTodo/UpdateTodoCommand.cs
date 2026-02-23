using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.UpdateTodo;

public record UpdateTodoCommand(int Id, string Title, bool IsCompleted, int? CategoryId, List<int> TagIds) : IRequest<TodoItemResponse?>;