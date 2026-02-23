using MediatR;

namespace TodoAPI.Application.Commands.DeleteTodo;

public record DeleteTodoCommand(int Id) : IRequest<bool>;