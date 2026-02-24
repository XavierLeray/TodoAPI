using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Exceptions;
using TodoAPI.Domain.Ports;
using TodoAPI.Infrastructure.Data;

namespace TodoAPI.Infrastructure.Repositories;

public class EfSparePartRepository : ISparePartRepository
{
    private readonly AppDbContext _context;

    public EfSparePartRepository(AppDbContext context)
    {
        _context = context;
    }

    public async Task<SparePart?> GetByIdAsync(int id, CancellationToken ct = default)
        => await _context.SpareParts.FindAsync([id], ct);

    public async Task<List<SparePart>> GetAllAsync(CancellationToken ct = default)
        => await _context.SpareParts.ToListAsync(ct);

    public async Task<SparePart> CreateAsync(SparePart part, CancellationToken ct = default)
    {
        _context.SpareParts.Add(part);
        await _context.SaveChangesAsync(ct);
        return part;
    }

    public async Task<SparePart?> ReserveAsync(int id, string channel, string buyer, string concurrencyStamp, CancellationToken ct = default)
    {
        var part = await _context.SpareParts.FindAsync([id], ct);
        if (part is null) return null;

        // Vérification manuelle du ConcurrencyStamp
        // (fonctionne avec InMemory ET SQLite/SQL Server)
        if (part.ConcurrencyStamp != concurrencyStamp)
        {
            throw new ConcurrencyConflictException(nameof(SparePart), id);
        }

        // Vérifier que la pièce est disponible et en stock
        if (part.Status != PartStatus.Available || part.StockQuantity <= 0)
        {
            throw new ConcurrencyConflictException(nameof(SparePart), id);
        }

        // Appliquer le ConcurrencyStamp reçu du client pour la détection EF Core
        _context.Entry(part).Property(e => e.ConcurrencyStamp).OriginalValue = concurrencyStamp;

        // Mettre à jour la pièce
        part.Status = PartStatus.Reserved;
        part.StockQuantity--;
        part.ReservedByChannel = channel;
        part.ReservedByBuyer = buyer;
        part.ReservedAt = DateTime.UtcNow;
        part.ConcurrencyStamp = Guid.NewGuid().ToString();

        try
        {
            await _context.SaveChangesAsync(ct);
        }
        catch (DbUpdateConcurrencyException)
        {
            throw new ConcurrencyConflictException(nameof(SparePart), id);
        }

        return part;
    }
}
