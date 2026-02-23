using FluentValidation;
using TodoAPI.Application.DTOs;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Validators;

public class CreateTodoRequestValidator : AbstractValidator<CreateTodoRequest>
{
    public CreateTodoRequestValidator(ITodoRepository repository)
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MinimumLength(3).WithMessage("Title must be at least 3 characters")
            .MaximumLength(200).WithMessage("Title must not exceed 200 characters")
            .Must(title => !title.Contains("spam")).WithMessage("Title must not contain 'spam'")
            .MustAsync(async (title, cancellationToken) =>
            {
                var exists = await repository.ExistsByTitleAsync(title, cancellationToken);
                return !exists;
            }).WithMessage("A todo with this title already exists");
    }
}