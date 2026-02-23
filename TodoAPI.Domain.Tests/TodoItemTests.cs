using FluentAssertions;
using TodoAPI.Domain.Entities;

namespace TodoAPI.Domain.Tests;

public class TodoItemTests
{
    [Theory]
    [InlineData("Buy groceries", true)]
    [InlineData("abc", true)]
    [InlineData("ab", false)]
    [InlineData("", false)]
    [InlineData("This is spam", false)]
    public void IsTitleValid_Should_Validate_Correctly(string title, bool expected)
    {
        TodoItem.IsTitleValid(title).Should().Be(expected);
    }

    [Fact]
    public void IsTitleValid_Should_Reject_Title_Over_200_Characters()
    {
        var longTitle = new string('a', 201);
        TodoItem.IsTitleValid(longTitle).Should().BeFalse();
    }

    [Fact]
    public void Create_Should_Return_New_TodoItem_With_Correct_Defaults()
    {
        var before = DateTime.UtcNow;
        var todo = TodoItem.Create("Test todo");
        var after = DateTime.UtcNow;

        todo.Title.Should().Be("Test todo");
        todo.IsCompleted.Should().BeFalse();
        todo.CreatedAt.Should().BeOnOrAfter(before).And.BeOnOrBefore(after);
    }
}