namespace TodoAPI.Application.DTOs;

public class TodoItemResponse
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }
    public CategoryDto? Category { get; set; }
    public List<TagDto> Tags { get; set; } = new();

    /// <summary>
    /// Concurrency token (GUID) — à renvoyer dans le PUT pour détecter les conflits.
    /// </summary>
    public string? ConcurrencyStamp { get; set; }
}

public class CategoryDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = string.Empty;
}

public class TagDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
}