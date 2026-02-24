namespace TodoAPI.Application.DTOs;

public class ReservePartRequest
{
    public string Channel { get; set; } = string.Empty;
    public string BuyerName { get; set; } = string.Empty;
    public string ConcurrencyStamp { get; set; } = string.Empty;
}
