using MediatR;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Commands.DeleteTodo;

public class DeleteTodoCommandHandler : IRequestHandler<DeleteTodoCommand, bool>
{
    private readonly ITodoRepository _repository;

    public DeleteTodoCommandHandler(ITodoRepository repository)
    {
        _repository = repository;
    }

    public async Task<bool> Handle(DeleteTodoCommand request, CancellationToken cancellationToken)
    {
        return await _repository.DeleteAsync(request.Id, cancellationToken);
    }
}