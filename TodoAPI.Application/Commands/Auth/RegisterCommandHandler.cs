using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Constants;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.Auth;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, AuthResponse?>
{
    private readonly IUserRepository _userRepository;
    private readonly IJwtService _jwtService;

    public RegisterCommandHandler(IUserRepository userRepository, IJwtService jwtService)
    {
        _userRepository = userRepository;
        _jwtService = jwtService;
    }

    public async Task<AuthResponse?> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var existingUser = await _userRepository.GetByUsernameAsync(request.Username, cancellationToken);
        if (existingUser is not null) return null;

        var existingEmail = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
        if (existingEmail is not null) return null;

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        var created = await _userRepository.CreateAsync(user, cancellationToken);
        await _userRepository.AddUserRoleAsync(created.Id, 2, cancellationToken); // Role "User" = Id 2

        var roles = new List<string> { Roles.User };
        var token = _jwtService.GenerateToken(created.Id, created.Username, roles);

        return new AuthResponse
        {
            Token = token,
            Username = created.Username,
            Roles = roles
        };
    }
}