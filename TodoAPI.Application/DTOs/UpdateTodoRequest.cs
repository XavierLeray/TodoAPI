namespace TodoAPI.Application.DTOs;

public class UpdateTodoRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
}