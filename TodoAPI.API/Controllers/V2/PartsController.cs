using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Application.Commands.ReservePart;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Queries.GetAllParts;
using TodoAPI.Application.Queries.GetPartById;
using TodoAPI.Domain.Exceptions;

namespace TodoAPI.API.Controllers.V2;

[ApiVersion("2.0")]
[Route("api/v{version:apiVersion}/parts")]
[ApiController]
public class PartsController : ControllerBase
{
    private readonly IMediator _mediator;

    public PartsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    [ProducesResponseType(typeof(List<SparePartResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<SparePartResponse>>> GetAll()
    {
        var parts = await _mediator.Send(new GetAllPartsQuery());
        return Ok(parts);
    }

    [HttpGet("{id}")]
    [ProducesResponseType(typeof(SparePartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<SparePartResponse>> GetById(int id)
    {
        var part = await _mediator.Send(new GetPartByIdQuery(id));
        return part is null ? NotFound() : Ok(part);
    }

    [HttpPost("{id}/reserve")]
    [ProducesResponseType(typeof(SparePartResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<SparePartResponse>> Reserve(int id, [FromBody] ReservePartRequest request)
    {
        try
        {
            var part = await _mediator.Send(new ReservePartCommand(
                id, request.Channel, request.BuyerName, request.ConcurrencyStamp));

            return part is null ? NotFound() : Ok(part);
        }
        catch (ConcurrencyConflictException ex)
        {
            return Conflict(new
            {
                error = "ConcurrencyConflict",
                message = ex.Message,
                entityId = ex.EntityId
            });
        }
    }
}
