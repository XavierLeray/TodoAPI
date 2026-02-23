using Asp.Versioning;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.API.Controllers.V1;

[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/todos")]
[ApiController]
public class TodosController : ControllerBase
{
    private readonly ITodoRepository _repository;

    public TodosController(ITodoRepository repository)
    {
        _repository = repository;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<TodoItem>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<TodoItem>>> GetAll(CancellationToken cancellationToken)
    {
        var todos = await _repository.GetAllAsync(cancellationToken);
        return Ok(todos);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> GetById(int id, CancellationToken cancellationToken)
    {
        var todo = await _repository.GetByIdAsync(id, cancellationToken);
        return todo is null ? NotFound() : Ok(todo);
    }

    [HttpPost]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<TodoItem>> Create([FromBody] CreateTodoSimpleRequest request, CancellationToken cancellationToken)
    {
        if (!TodoItem.IsTitleValid(request.Title))
            return BadRequest("Invalid title: must be 3-200 characters and not contain 'spam'");

        var todo = TodoItem.Create(request.Title);
        var created = await _repository.CreateAsync(todo, cancellationToken);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    [ProducesResponseType(typeof(TodoItem), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TodoItem>> Update(int id, [FromBody] UpdateTodoSimpleRequest request, CancellationToken cancellationToken)
    {
        var todo = new TodoItem
        {
            Title = request.Title,
            IsCompleted = request.IsCompleted
        };

        var updated = await _repository.UpdateAsync(id, todo, cancellationToken);
        return updated is null ? NotFound() : Ok(updated);
    }

    [HttpDelete("{id}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult> Delete(int id, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(id, cancellationToken);
        return deleted ? NoContent() : NotFound();
    }
}

public record CreateTodoSimpleRequest(string Title);
public record UpdateTodoSimpleRequest(string Title, bool IsCompleted);