using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.Auth;

public record LoginCommand(string Username, string Password) : IRequest<AuthResponse?>;