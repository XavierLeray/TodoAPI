using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.Auth;

public record RegisterCommand(string Username, string Email, string Password) : IRequest<AuthResponse?>;