using Asp.Versioning;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using TodoAPI.Application.Commands.Auth;
using TodoAPI.Application.DTOs;

namespace TodoAPI.API.Controllers;

[ApiVersionNeutral]
[Route("api/auth")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly IMediator _mediator;

    public AuthController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpPost("register")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<AuthResponse>> Register([FromBody] RegisterRequest request)
    {
        var result = await _mediator.Send(new RegisterCommand(request.Username, request.Email, request.Password));
        return result is null ? BadRequest("Username or email already exists") : Ok(result);
    }

    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
    {
        var result = await _mediator.Send(new LoginCommand(request.Username, request.Password));
        return result is null ? Unauthorized("Invalid credentials") : Ok(result);
    }
}