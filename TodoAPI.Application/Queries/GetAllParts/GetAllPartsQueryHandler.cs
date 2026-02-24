using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Queries.GetAllParts;

public class GetAllPartsQueryHandler : IRequestHandler<GetAllPartsQuery, List<SparePartResponse>>
{
    private readonly ISparePartRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "parts:all";

    public GetAllPartsQueryHandler(ISparePartRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<List<SparePartResponse>> Handle(GetAllPartsQuery request, CancellationToken cancellationToken)
    {
        var cached = await _cache.GetAsync<List<SparePartResponse>>(CacheKey, cancellationToken);
        if (cached is not null) return cached;

        var parts = await _repository.GetAllAsync(cancellationToken);
        var response = parts.Select(MapToResponse).ToList();

        await _cache.SetAsync(CacheKey, response, TimeSpan.FromMinutes(5), cancellationToken);

        return response;
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
