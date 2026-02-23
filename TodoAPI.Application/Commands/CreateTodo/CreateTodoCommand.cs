using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.CreateTodo;

public record CreateTodoCommand(string Title, int? CategoryId, List<int> TagIds) : IRequest<TodoItemResponse>;