namespace TodoAPI.Domain.Entities;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Règles métier pures dans le Domain
    public static bool IsTitleValid(string title)
        => !string.IsNullOrEmpty(title)
        && title.Length >= 3
        && title.Length <= 200
        && !title.Contains("spam");

    public static TodoItem Create(string title)
    {
        return new TodoItem
        {
            Title = title,
            IsCompleted = false,
            CreatedAt = DateTime.UtcNow
        };
    }
}