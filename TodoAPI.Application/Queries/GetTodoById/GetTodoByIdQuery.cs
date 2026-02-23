using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Queries.GetTodoById;

public record GetTodoByIdQuery(int Id) : IRequest<TodoItemResponse?>;