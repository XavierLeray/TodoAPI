using MediatR;
using TodoAPI.Application.DTOs;

namespace TodoAPI.Application.Queries.GetAllParts;

public record GetAllPartsQuery() : IRequest<List<SparePartResponse>>;
