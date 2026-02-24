namespace TodoAPI.Application.DTOs;

public class UpdateTodoRequest
{
    public int Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public bool IsCompleted { get; set; }
    public int? CategoryId { get; set; }
    public List<int> TagIds { get; set; } = new();

    /// <summary>
    /// Concurrency token (GUID) reçu lors du GET, renvoyé lors du PUT.
    /// Si absent ou vide, pas de vérification de concurrence (rétrocompatibilité).
    /// </summary>
    public string? ConcurrencyStamp { get; set; }
}