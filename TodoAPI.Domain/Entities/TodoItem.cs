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

    // Concurrency token — détecte les modifications concurrentes
    // EF Core vérifie ce champ dans la clause WHERE du UPDATE.
    // Si la valeur a changé entre le SELECT et le UPDATE → DbUpdateConcurrencyException.
    //
    // On utilise un string (GUID) au lieu d'un byte[] (rowversion SQL Server) :
    //   - Portable : fonctionne avec SQLite, InMemory, PostgreSQL, SQL Server…
    //   - Sérialisable : pas besoin d'encoder/décoder en Base64 pour le JSON
    //   - Le repository génère un nouveau GUID à chaque UPDATE
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();

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