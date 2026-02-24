namespace TodoAPI.Domain.Entities;

public enum PartStatus
{
    Available,
    Reserved,
    Sold
}

public class SparePart
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VhuCenter { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; } = 1;
    public PartStatus Status { get; set; } = PartStatus.Available;
    public string? ReservedByChannel { get; set; }
    public string? ReservedByBuyer { get; set; }
    public DateTime? ReservedAt { get; set; }
    public DateTime CreatedAt { get; set; }

    // Concurrency token — même principe que TodoItem
    // EF Core vérifie ce champ dans la clause WHERE du UPDATE.
    // Si la valeur a changé entre le SELECT et le UPDATE → DbUpdateConcurrencyException.
    public string ConcurrencyStamp { get; set; } = Guid.NewGuid().ToString();
}
