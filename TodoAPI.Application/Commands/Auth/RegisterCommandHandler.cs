using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
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
        if (await _userRepository.UsernameExistsAsync(request.Username, cancellationToken))
            throw new ArgumentException("Username already exists");

        if (await _userRepository.EmailExistsAsync(request.Email, cancellationToken))
            throw new ArgumentException("Email already exists");

        var user = new User
        {
            Username = request.Username,
            Email = request.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            CreatedAt = DateTime.UtcNow
        };

        await _userRepository.CreateAsync(user, cancellationToken);
        await _userRepository.AssignRoleAsync(user.Id, 2, cancellationToken);

        var roles = new List<string> { "User" };
        var token = _jwtService.GenerateToken(user.Id, user.Username, roles);

        return new AuthResponse
        {
            Token = token,
            Username = user.Username,
            Roles = roles
        };
    }
}