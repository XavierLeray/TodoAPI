using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Queries.GetPartById;

public record GetPartByIdQuery(int Id) : IRequest<SparePartResponse?>;
