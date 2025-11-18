// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.Common;

namespace TodoListManager.Domain.Tests.Common;

/// <summary>
/// Unit tests for Result pattern implementation following best practices.
/// </summary>
public class ResultTests
{
    #region Success Tests

    [Fact]
    public void Success_ShouldCreateSuccessfulResult()
    {
        // Act
        var result = Result.Success();

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
    }

    [Fact]
    public void Success_WithValue_ShouldCreateSuccessfulResultWithValue()
    {
        // Arrange
        var expectedValue = "test value";

        // Act
        var result = Result.Success(expectedValue);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeEmpty();
        result.Value.Should().Be(expectedValue);
    }

    [Theory]
    [InlineData(42)]
    [InlineData("string value")]
    [InlineData(true)]
    public void Success_WithVariousTypes_ShouldCreateSuccessfulResult(object value)
    {
        // Act
        var result = Result.Success(value);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(value);
    }

    [Fact]
    public void Success_WithNullValue_ShouldStillBeSuccess()
    {
        // Act
        var result = Result.Success<string>(null!);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeNull();
    }

    #endregion

    #region Failure Tests

    [Fact]
    public void Failure_WithErrorMessage_ShouldCreateFailedResult()
    {
        // Arrange
        var errorMessage = "Something went wrong";

        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Should().NotBeNull();
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
    }

    [Fact]
    public void Failure_WithValue_ShouldCreateFailedResultWithDefaultValue()
    {
        // Arrange
        var errorMessage = "Operation failed";

        // Act
        var result = Result.Failure<int>(errorMessage);

        // Assert
        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(errorMessage);
        result.Value.Should().Be(default(int));
    }

    [Theory]
    [InlineData("User not found")]
    [InlineData("Invalid credentials")]
    [InlineData("Database connection error")]
    public void Failure_WithVariousErrorMessages_ShouldStoreCorrectMessage(string errorMessage)
    {
        // Act
        var result = Result.Failure(errorMessage);

        // Assert
        result.Error.Should().Be(errorMessage);
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Constructor_SuccessWithError_ShouldThrowInvalidOperationException()
    {
        // Act
        Action act = () => new TestableResult(true, "error message");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*successful result cannot have an error*");
    }

    [Fact]
    public void Constructor_FailureWithoutError_ShouldThrowInvalidOperationException()
    {
        // Act
        Action act = () => new TestableResult(false, string.Empty);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*failed result must have an error*");
    }

    #endregion

    #region IsFailure Property Tests

    [Fact]
    public void IsFailure_ForSuccessResult_ShouldBeFalse()
    {
        // Arrange
        var result = Result.Success();

        // Act & Assert
        result.IsFailure.Should().BeFalse();
    }

    [Fact]
    public void IsFailure_ForFailureResult_ShouldBeTrue()
    {
        // Arrange
        var result = Result.Failure("error");

        // Act & Assert
        result.IsFailure.Should().BeTrue();
    }

    [Fact]
    public void IsSuccess_AndIsFailure_ShouldBeOpposites()
    {
        // Arrange
        var successResult = Result.Success();
        var failureResult = Result.Failure("error");

        // Assert
        successResult.IsSuccess.Should().Be(!successResult.IsFailure);
        failureResult.IsSuccess.Should().Be(!failureResult.IsFailure);
    }

    #endregion

    #region Generic Result Tests

    [Fact]
    public void GenericResult_ShouldInheritFromBaseResult()
    {
        // Arrange
        var result = Result.Success(42);

        // Assert
        result.Should().BeAssignableTo<Result>();
    }

    [Fact]
    public void GenericResult_WithComplexType_ShouldWorkCorrectly()
    {
        // Arrange
        var complexObject = new { Name = "Test", Value = 123 };

        // Act
        var result = Result.Success(complexObject);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().BeEquivalentTo(complexObject);
    }

    [Fact]
    public void GenericResult_WithReferenceType_ShouldMaintainReference()
    {
        // Arrange
        var list = new List<int> { 1, 2, 3 };

        // Act
        var result = Result.Success(list);

        // Assert
        result.Value.Should().BeSameAs(list);
    }

    #endregion

    #region Edge Cases

    [Fact]
    public void Failure_WithEmptyStringError_ShouldThrow()
    {
        // Act
        Action act = () => Result.Failure(string.Empty);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void GenericFailure_WithEmptyStringError_ShouldThrow()
    {
        // Act
        Action act = () => Result.Failure<int>(string.Empty);

        // Assert
        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Success_CalledMultipleTimes_ShouldCreateIndependentResults()
    {
        // Act
        var result1 = Result.Success();
        var result2 = Result.Success();

        // Assert
        result1.Should().NotBeSameAs(result2);
    }

    [Fact]
    public void GenericSuccess_WithSameValue_ShouldCreateIndependentResults()
    {
        // Act
        var result1 = Result.Success(42);
        var result2 = Result.Success(42);

        // Assert
        result1.Should().NotBeSameAs(result2);
        result1.Value.Should().Be(result2.Value);
    }

    #endregion

    #region Real World Usage Scenarios

    [Fact]
    public void Result_UsedInValidationScenario_ShouldWorkCorrectly()
    {
        // Simulate validation logic
        Result ValidateUser(string username, string password)
        {
            if (string.IsNullOrEmpty(username))
                return Result.Failure("Username is required");
            
            if (string.IsNullOrEmpty(password))
                return Result.Failure("Password is required");
            
            return Result.Success();
        }

        // Test valid scenario
        var validResult = ValidateUser("user", "pass");
        validResult.IsSuccess.Should().BeTrue();

        // Test invalid scenarios
        var invalidResult1 = ValidateUser("", "pass");
        invalidResult1.IsFailure.Should().BeTrue();
        invalidResult1.Error.Should().Contain("Username");

        var invalidResult2 = ValidateUser("user", "");
        invalidResult2.IsFailure.Should().BeTrue();
        invalidResult2.Error.Should().Contain("Password");
    }

    [Fact]
    public void GenericResult_UsedInRepositoryScenario_ShouldWorkCorrectly()
    {
        // Simulate repository method
        Result<User> GetUserById(int id)
        {
            if (id <= 0)
                return Result.Failure<User>("Invalid user ID");
            
            if (id == 999)
                return Result.Failure<User>("User not found");
            
            return Result.Success(new User { Id = id, Name = "Test User" });
        }

        // Test success scenario
        var successResult = GetUserById(1);
        successResult.IsSuccess.Should().BeTrue();
        successResult.Value.Should().NotBeNull();
        successResult.Value.Id.Should().Be(1);

        // Test failure scenarios
        var failureResult1 = GetUserById(-1);
        failureResult1.IsFailure.Should().BeTrue();
        failureResult1.Error.Should().Contain("Invalid");

        var failureResult2 = GetUserById(999);
        failureResult2.IsFailure.Should().BeTrue();
        failureResult2.Error.Should().Contain("not found");
    }

    [Fact]
    public void Result_ChainedOperations_ShouldMaintainState()
    {
        // Arrange
        Result ProcessData(bool shouldSucceed)
        {
            if (!shouldSucceed)
                return Result.Failure("Processing failed");
            
            return Result.Success();
        }

        Result<string> TransformData(Result previousResult, string data)
        {
            if (previousResult.IsFailure)
                return Result.Failure<string>(previousResult.Error);
            
            return Result.Success(data.ToUpper());
        }

        // Act - Success path
        var result1 = ProcessData(true);
        var result2 = TransformData(result1, "test");

        // Assert - Success path
        result2.IsSuccess.Should().BeTrue();
        result2.Value.Should().Be("TEST");

        // Act - Failure path
        var result3 = ProcessData(false);
        var result4 = TransformData(result3, "test");

        // Assert - Failure path
        result4.IsFailure.Should().BeTrue();
        result4.Error.Should().Contain("Processing failed");
    }

    #endregion

    #region Helper Classes for Testing

    private class TestableResult : Result
    {
        public TestableResult(bool isSuccess, string error) : base(isSuccess, error)
        {
        }
    }

    private class User
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }

    #endregion
}
