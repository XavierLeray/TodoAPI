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
        var command = new CreateTodoCommand("Buy groceries", null, new List<int>());

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem todo, CancellationToken _) =>
            {
                todo.Id = 1;
                return todo;
            });

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TodoItem
            {
                Id = 1,
                Title = "Buy groceries",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                TodoItemTags = new List<TodoItemTag>()
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Title.Should().Be("Buy groceries");
        result.IsCompleted.Should().BeFalse();
        result.Tags.Should().BeEmpty();
        result.Category.Should().BeNull();

        _repositoryMock.Verify(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()), Times.Once);
        _cacheMock.Verify(c => c.RemoveAsync("todos:all", It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Handle_Should_Create_Todo_With_Category_And_Tags()
    {
        // Arrange
        var command = new CreateTodoCommand("Work task", 2, new List<int> { 1, 3 });

        _repositoryMock
            .Setup(r => r.CreateAsync(It.IsAny<TodoItem>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((TodoItem todo, CancellationToken _) =>
            {
                todo.Id = 1;
                return todo;
            });

        _repositoryMock
            .Setup(r => r.GetByIdWithRelationsAsync(1, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new TodoItem
            {
                Id = 1,
                Title = "Work task",
                IsCompleted = false,
                CreatedAt = DateTime.UtcNow,
                CategoryId = 2,
                Category = new Category { Id = 2, Name = "Travail", Color = "#e74c3c" },
                TodoItemTags = new List<TodoItemTag>
                {
                    new() { TagId = 1, Tag = new Tag { Id = 1, Name = "urgent" } },
                    new() { TagId = 3, Tag = new Tag { Id = 3, Name = "facile" } }
                }
            });

        // Act
        var result = await _handler.Handle(command, CancellationToken.None);

        // Assert
        result.Should().NotBeNull();
        result.Category.Should().NotBeNull();
        result.Category!.Name.Should().Be("Travail");
        result.Tags.Should().HaveCount(2);
        result.Tags.Select(t => t.Name).Should().Contain("urgent");
        result.Tags.Select(t => t.Name).Should().Contain("facile");
    }
}
