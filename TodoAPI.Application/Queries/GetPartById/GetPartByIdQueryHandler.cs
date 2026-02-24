using MediatR;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Queries.GetPartById;

public class GetPartByIdQueryHandler : IRequestHandler<GetPartByIdQuery, SparePartResponse?>
{
    private readonly ISparePartRepository _repository;

    public GetPartByIdQueryHandler(ISparePartRepository repository)
    {
        _repository = repository;
    }

    public async Task<SparePartResponse?> Handle(GetPartByIdQuery request, CancellationToken cancellationToken)
    {
        var part = await _repository.GetByIdAsync(request.Id, cancellationToken);
        if (part is null) return null;

        return new SparePartResponse
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
}
