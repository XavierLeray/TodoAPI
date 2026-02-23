using Asp.Versioning;
using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Application.Commands.CreateTodo;
using TodoAPI.Application.Commands.DeleteTodo;
using TodoAPI.Application.Commands.UpdateTodo;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Queries.GetAllTodos;
using TodoAPI.Application.Queries.GetTodoById;
using TodoAPI.Domain.Constants;

namespace TodoAPI.API.Controllers.V2;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/todos")]
[ApiController]
public class TodosController : ControllerBase
{
    private readonly IMediator _mediator;
    private readonly IValidator<CreateTodoRequest> _validator;

    public TodosController(IMediator mediator, IValidator<CreateTodoRequest> validator)
    {
        _mediator = mediator;
        _validator = validator;
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

    [Authorize]
    [HttpPost]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TodoItemResponse>> Create([FromBody] CreateTodoRequest request)
    {
        var validationResult = await _validator.ValidateAsync(request);
        if (!validationResult.IsValid)
        {
            var errors = validationResult.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray()
                );
            return ValidationProblem(new ValidationProblemDetails(errors));
        }

        var todo = await _mediator.Send(new CreateTodoCommand(request.Title, request.CategoryId, request.TagIds));
        return CreatedAtAction(nameof(GetById), new { id = todo.Id }, todo);
    }

    [Authorize]
    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoItemResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<TodoItemResponse>> Update(int id, [FromBody] UpdateTodoRequest request)
    {
        if (id != request.Id)
            return BadRequest("ID mismatch");

        var updated = await _mediator.Send(new UpdateTodoCommand(
            request.Id, request.Title, request.IsCompleted, request.CategoryId, request.TagIds));
        return updated is null ? NotFound() : Ok(updated);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult> Delete(int id)
    {
        var deleted = await _mediator.Send(new DeleteTodoCommand(id));
        return deleted ? NoContent() : NotFound();
    }
}