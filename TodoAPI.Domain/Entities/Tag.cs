namespace TodoAPI.Domain.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public ICollection<TodoItemTag> TodoItemTags { get; set; } = new List<TodoItemTag>();
}