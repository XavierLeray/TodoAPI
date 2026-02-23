using FluentAssertions;
using Moq;
using TodoAPI.Application.DTOs;
using TodoAPI.Application.Validators;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Tests;

public class CreateTodoRequestValidatorTests
{
    private readonly Mock<ITodoRepository> _repositoryMock;
    private readonly CreateTodoRequestValidator _validator;

    public CreateTodoRequestValidatorTests()
    {
        _repositoryMock = new Mock<ITodoRepository>();
        _repositoryMock
            .Setup(r => r.ExistsByTitleAsync(It.IsAny<string>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        _validator = new CreateTodoRequestValidator(_repositoryMock.Object);
    }

    [Fact]
    public async Task Should_Pass_When_Title_Is_Valid()
    {
        var request = new CreateTodoRequest { Title = "Buy groceries" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeTrue();
    }

    [Fact]
    public async Task Should_Fail_When_Title_Is_Empty()
    {
        var request = new CreateTodoRequest { Title = "" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.PropertyName == "Title");
    }

    [Fact]
    public async Task Should_Fail_When_Title_Is_Too_Short()
    {
        var request = new CreateTodoRequest { Title = "ab" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Title_Exceeds_200_Characters()
    {
        var request = new CreateTodoRequest { Title = new string('a', 201) };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Title_Contains_Spam()
    {
        var request = new CreateTodoRequest { Title = "This is spam content" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
    }

    [Fact]
    public async Task Should_Fail_When_Title_Already_Exists()
    {
        _repositoryMock
            .Setup(r => r.ExistsByTitleAsync("Existing title", It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);

        var request = new CreateTodoRequest { Title = "Existing title" };
        var result = await _validator.ValidateAsync(request);
        result.IsValid.Should().BeFalse();
        result.Errors.Should().Contain(e => e.ErrorMessage == "A todo with this title already exists");
    }
}