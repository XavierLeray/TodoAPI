using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Commands.ReservePart;

public record ReservePartCommand(int PartId, string Channel, string BuyerName, string ConcurrencyStamp) : IRequest<SparePartResponse?>;
