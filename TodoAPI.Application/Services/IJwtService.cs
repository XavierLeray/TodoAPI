namespace TodoAPI.Application.Services;

public interface IJwtService
{
    string GenerateToken(int userId, string username, List<string> roles);
}