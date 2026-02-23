namespace TodoAPI.Application.DTOs;

public class CreateTodoRequest
{
    public string Title { get; set; } = string.Empty;
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();
}