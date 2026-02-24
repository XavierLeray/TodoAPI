namespace TodoAPI.Application.DTOs;

public class SparePartResponse
{
    public int Id { get; set; }
    public string Reference { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VhuCenter { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public int StockQuantity { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ReservedByChannel { get; set; }
    public string? ReservedByBuyer { get; set; }
    public DateTime? ReservedAt { get; set; }
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
