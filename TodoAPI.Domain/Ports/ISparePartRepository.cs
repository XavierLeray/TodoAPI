using TodoAPI.Domain.Entities;

namespace TodoAPI.Domain.Ports;

public interface ISparePartRepository
{
    Task<SparePart?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<List<SparePart>> GetAllAsync(CancellationToken ct = default);
    Task<SparePart> CreateAsync(SparePart part, CancellationToken ct = default);

    /// <summary>
    /// Réserve une pièce pour un acheteur sur un canal donné.
    /// Vérifie que Status == Available ET StockQuantity > 0,
    /// puis passe Status à Reserved, décrémente StockQuantity,
    /// et met à jour le ConcurrencyStamp.
    /// Si conflit de concurrence → throw ConcurrencyConflictException.
    /// </summary>
    Task<SparePart?> ReserveAsync(int id, string channel, string buyer, string concurrencyStamp, CancellationToken ct = default);
}
