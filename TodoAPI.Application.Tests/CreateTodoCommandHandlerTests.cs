using FluentAssertions;
using Moq;
using TodoAPI.Application.Commands.CreateTodo;
using TodoAPI.Application.Services;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Ports;

namespace TodoAPI.Application.Tests;

public class CreateTodoCommandHandlerTests
{
    private readonly Mock<ITodoRepository> _repositoryMock;
    private readonly Mock<ICacheService> _cacheMock;
    private readonly CreateTodoCommandHandler _handler;

    public CreateTodoCommandHandlerTests()
    {
        _repositoryMock = new Mock<ITodoRepository>();
        _cacheMock = new Mock<ICacheService>();
        _handler = new CreateTodoCommandHandler(_repositoryMock.Object, _cacheMock.Object);
    }

    [Fact]
    public async Task Handle_Should_Create_Todo_And_Invalidate_Cache()
    {
        // Arrange
        var command = new CreateTodoCommand("Buy groceries");

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem todo, CancellationToken _) =>
            {
                todo.Id = 1;
                return todo;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Buy groceries");
        result.IsCompleted.Should().BeFalse();

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("todos:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Set_CreatedAt_To_UtcNow()
    {
        // Arrange
        var before = DateTime.UtcNow;
        var command = new CreateTodoCommand("Test todo");

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem todo, CancellationToken _) =>
            {
                todo.Id = 1;
                return todo;
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);
        var after = DateTime.UtcNow;

        // Assert
        result.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}