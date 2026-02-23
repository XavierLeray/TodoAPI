using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.UpdateTodo;

public record UpdateTodoCommand(int Id, string Title, bool IsCompleted) : IRequest<TodoItemResponse?>;