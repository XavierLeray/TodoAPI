using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Application.Commands.CreateTodo;
using TodoAPI.Application.Commands.DeleteTodo;
using TodoAPI.Application.Commands.UpdateTodo;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Queries.GetAllTodos;
using TodoAPI.Application.Queries.GetTodoById;

namespace TodoAPI.API.Controllers;

[Route("api/todos")]
[ApiController]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;

    public TodosController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TodoItemResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TodoItemResponse>>> GetAll()
    {
        var todos = await _mediator.Send(new GetAllTodosQuery());
        return Ok(todos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemResponse>> GetById(int id)
    {
        var todo = await _mediator.Send(new GetTodoByIdQuery(id));
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItemResponse>> Create([FromBody] CreateTodoRequest request)
    {
        var todo = await _mediator.Send(new CreateTodoCommand(request.Title));
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItemResponse>> Update(int id, [FromBody] UpdateTodoRequest request)
    {
        if (id != request.Id)
            return BadRequest("ID mismatch");

        var updated = await _mediator.Send(new UpdateTodoCommand(request.Id, request.Title, request.IsCompleted));
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(new DeleteTodoCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}