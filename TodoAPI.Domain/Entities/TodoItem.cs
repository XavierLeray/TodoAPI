namespace TodoAPI.Domain.Entities;

public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public DateTime CreatedAt { get; set; }

    // Relation N-1 : Un todo appartient à UNE catégorie (optionnel)
    public int? CategoryId { get; set; }
    public Category? Category { get; set; }

    // Relation N-N : Un todo peut avoir plusieurs tags
    public ICollection<TodoItemTag> TodoItemTags { get; set; } = new List<TodoItemTag>();

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