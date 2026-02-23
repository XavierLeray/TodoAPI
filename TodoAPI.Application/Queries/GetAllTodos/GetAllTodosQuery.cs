using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Queries.GetAllTodos;

public record GetAllTodosQuery() : IRequest<List<TodoItemResponse>>;