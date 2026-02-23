using MediatR;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.DeleteTodo;

public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly ITodoRepository _repository;
    private readonly ICacheService _cache;
    private const string CacheKey = "todos:all";

    public DeleteTodoCommandHandler(ITodoRepository repository, ICacheService cache)
    {
        _repository = repository;
        _cache = cache;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        var deleted = await _repository.DeleteAsync(request.Id, cancellationToken);

        if (deleted)
        {
            await _cache.RemoveAsync(CacheKey, cancellationToken);
        }

        return deleted;
    }
}