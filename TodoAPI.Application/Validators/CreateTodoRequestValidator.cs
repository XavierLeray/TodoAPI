using FluentValidation;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Validators;

public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    private readonly ITodoRepository _repository;

    public CreateTodoRequestValidator(ITodoRepository repository)
    {
        _repository = repository;

        RuleFor(x => x.Title)
            .Cascade(CascadeMode.Stop)
            .Must(TodoItem.IsTitleValid)
            .WithMessage("Titre invalide (min 3, max 200 caractères, sans 'spam')")
            .MustAsync(async (title, ct) =>
                !await _repository.ExistsByTitleAsync(title, ct))
            .WithMessage("Un todo avec ce titre existe déjà");

        RuleFor(x => x.CategoryId)
            .Must(id => id == null || id > 0)
            .WithMessage("CategoryId doit être un nombre positif si fourni");

        RuleFor(x => x.TagIds)
            .Must(ids => ids == null || ids.All(id => id > 0))
            .WithMessage("Tous les TagIds doivent être des nombres positifs");
    }
}