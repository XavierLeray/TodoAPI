using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.ReservePart;

public class ReservePartCommandHandler : IRequestHandler<ReservePartCommand, SparePartResponse?>
{
    private readonly ISparePartRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "parts:all";

    public ReservePartCommandHandler(ISparePartRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<SparePartResponse?> Handle(ReservePartCommand request, CancellationToken cancellationToken)
    {
        var part = await _repository.ReserveAsync(
            request.PartId, request.Channel, request.BuyerName,
            request.ConcurrencyStamp, cancellationToken);

        if (part is null) return null;

        await _cache.RemoveAsync(CacheKey, cancellationToken);

        return MapToResponse(part);
    }

    private static SparePartResponse MapToResponse(SparePart part) => new()
    {
        Id = part.Id,
        Reference = part.Reference,
        Description = part.Description,
        VhuCenter = part.VhuCenter,
        Price = part.Price,
        StockQuantity = part.StockQuantity,
        Status = part.Status.ToString(),
        ReservedByChannel = part.ReservedByChannel,
        ReservedByBuyer = part.ReservedByBuyer,
        ReservedAt = part.ReservedAt,
        ConcurrencyStamp = part.ConcurrencyStamp
    };
}
