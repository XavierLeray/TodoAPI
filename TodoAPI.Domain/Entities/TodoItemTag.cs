namespace TodoAPI.Domain.Entities;

public class TodoItemTag
{
    public int TodoItemId { get; set; }
    public TodoItem TodoItem { get; set; } = null!;
    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    public DateTime AssignedAt { get; set; }
}