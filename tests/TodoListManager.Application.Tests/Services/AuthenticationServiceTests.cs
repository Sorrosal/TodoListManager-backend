// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using Moq;
using TodoListManager.Application.Services;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Application.Tests.Services;

/// <summary>
/// Unit tests for AuthenticationService following best practices.
/// Uses Moq for mocking dependencies.
/// </summary>
public class AuthenticationServiceTests
{
    private readonly Mock<IUserRepository> _mockUserRepository;
    private readonly Mock<ITokenService> _mockTokenService;
    private readonly Mock<IPasswordHasher> _mockPasswordHasher;
    private readonly AuthenticationService _authenticationService;

    public AuthenticationServiceTests()
    {
        _mockUserRepository = new Mock<IUserRepository>();
        _mockTokenService = new Mock<ITokenService>();
        _mockPasswordHasher = new Mock<IPasswordHasher>();
        _authenticationService = new AuthenticationService(
            _mockUserRepository.Object,
            _mockTokenService.Object,
            _mockPasswordHasher.Object
        );
    }

    #region Constructor Tests

    [Fact]
    public void Constructor_WithValidDependencies_ShouldCreateInstance()
    {
        // Act
        var service = new AuthenticationService(
            _mockUserRepository.Object,
            _mockTokenService.Object,
            _mockPasswordHasher.Object
        );

        // Assert
        service.Should().NotBeNull();
    }

    [Fact]
    public void Constructor_WithNullUserRepository_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AuthenticationService(
            null!,
            _mockTokenService.Object,
            _mockPasswordHasher.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("userRepository");
    }

    [Fact]
    public void Constructor_WithNullTokenService_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AuthenticationService(
            _mockUserRepository.Object,
            null!,
            _mockPasswordHasher.Object
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("tokenService");
    }

    [Fact]
    public void Constructor_WithNullPasswordHasher_ShouldThrowArgumentNullException()
    {
        // Act
        Action act = () => new AuthenticationService(
            _mockUserRepository.Object,
            _mockTokenService.Object,
            null!
        );

        // Assert
        act.Should().Throw<ArgumentNullException>()
            .WithParameterName("passwordHasher");
    }

    #endregion

    #region Authenticate - Success Tests

    [Fact]
    public void Authenticate_WithValidCredentials_ShouldReturnSuccessWithToken()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var hashedPassword = "hashed_password";
        var expectedToken = "jwt_token_12345";

        var user = User.Create(1, username, hashedPassword, new List<Role> { Role.User });

        _mockUserRepository.Setup(x => x.GetByUsername(username)).Returns(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(password, hashedPassword)).Returns(true);
        _mockTokenService.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

        // Act
        var result = _authenticationService.Authenticate(username, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedToken);
        _mockUserRepository.Verify(x => x.GetByUsername(username), Times.Once);
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, hashedPassword), Times.Once);
        _mockTokenService.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public void Authenticate_WithValidCredentials_ShouldCallDependenciesInCorrectOrder()
    {
        // Arrange
        var username = "testuser";
        var password = "password123";
        var user = User.Create(1, username, "hash", new List<Role> { Role.User });
        var callOrder = new List<string>();

        _mockUserRepository.Setup(x => x.GetByUsername(username))
            .Returns(user)
            .Callback(() => callOrder.Add("GetByUsername"));

        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()))
            .Returns(true)
            .Callback(() => callOrder.Add("VerifyPassword"));

        _mockTokenService.Setup(x => x.GenerateToken(It.IsAny<User>()))
            .Returns("token")
            .Callback(() => callOrder.Add("GenerateToken"));

        // Act
        _authenticationService.Authenticate(username, password);

        // Assert
        callOrder.Should().ContainInOrder("GetByUsername", "VerifyPassword", "GenerateToken");
    }

    #endregion

    #region Authenticate - Validation Tests

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Authenticate_WithInvalidUsername_ShouldReturnFailure(string invalidUsername)
    {
        // Act
        var result = _authenticationService.Authenticate(invalidUsername, "password");

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Username is required");
        _mockUserRepository.Verify(x => x.GetByUsername(It.IsAny<string>()), Times.Never);
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void Authenticate_WithInvalidPassword_ShouldReturnFailure(string invalidPassword)
    {
        // Act
        var result = _authenticationService.Authenticate("username", invalidPassword);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Password is required");
        _mockUserRepository.Verify(x => x.GetByUsername(It.IsAny<string>()), Times.Never);
    }

    #endregion

    #region Authenticate - Failure Tests

    [Fact]
    public void Authenticate_WithNonExistentUser_ShouldReturnFailure()
    {
        // Arrange
        var username = "nonexistent";
        var password = "password";

        _mockUserRepository.Setup(x => x.GetByUsername(username)).Returns((User)null!);

        // Act
        var result = _authenticationService.Authenticate(username, password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid username or password");
        _mockUserRepository.Verify(x => x.GetByUsername(username), Times.Once);
        _mockPasswordHasher.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
        _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Authenticate_WithIncorrectPassword_ShouldReturnFailure()
    {
        // Arrange
        var username = "testuser";
        var password = "wrongpassword";
        var correctHash = "correct_hash";

        var user = User.Create(1, username, correctHash, new List<Role> { Role.User });

        _mockUserRepository.Setup(x => x.GetByUsername(username)).Returns(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(password, correctHash)).Returns(false);

        // Act
        var result = _authenticationService.Authenticate(username, password);

        // Assert
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Contain("Invalid username or password");
        _mockUserRepository.Verify(x => x.GetByUsername(username), Times.Once);
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, correctHash), Times.Once);
        _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    [Fact]
    public void Authenticate_WhenPasswordVerificationFails_ShouldNotGenerateToken()
    {
        // Arrange
        var username = "testuser";
        var password = "password";
        var user = User.Create(1, username, "hash", new List<Role> { Role.User });

        _mockUserRepository.Setup(x => x.GetByUsername(username)).Returns(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        _authenticationService.Authenticate(username, password);

        // Assert
        _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #endregion

    #region VerifyPassword Tests

    [Fact]
    public void VerifyPassword_WithMatchingPassword_ShouldReturnTrue()
    {
        // Arrange
        var password = "password123";
        var hash = "hashed_password";

        _mockPasswordHasher.Setup(x => x.VerifyPassword(password, hash)).Returns(true);

        // Act
        var result = _authenticationService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeTrue();
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, hash), Times.Once);
    }

    [Fact]
    public void VerifyPassword_WithNonMatchingPassword_ShouldReturnFalse()
    {
        // Arrange
        var password = "wrongpassword";
        var hash = "hashed_password";

        _mockPasswordHasher.Setup(x => x.VerifyPassword(password, hash)).Returns(false);

        // Act
        var result = _authenticationService.VerifyPassword(password, hash);

        // Assert
        result.Should().BeFalse();
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, hash), Times.Once);
    }

    [Fact]
    public void VerifyPassword_ShouldDelegateToPasswordHasher()
    {
        // Arrange
        var password = "test";
        var hash = "hash";

        // Act
        _authenticationService.VerifyPassword(password, hash);

        // Assert
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, hash), Times.Once);
    }

    #endregion

    #region HashPassword Tests

    [Fact]
    public void HashPassword_ShouldReturnHashedPassword()
    {
        // Arrange
        var password = "password123";
        var expectedHash = "hashed_password_12345";

        _mockPasswordHasher.Setup(x => x.HashPassword(password)).Returns(expectedHash);

        // Act
        var result = _authenticationService.HashPassword(password);

        // Assert
        result.Should().Be(expectedHash);
        _mockPasswordHasher.Verify(x => x.HashPassword(password), Times.Once);
    }

    [Fact]
    public void HashPassword_ShouldDelegateToPasswordHasher()
    {
        // Arrange
        var password = "test";

        // Act
        _authenticationService.HashPassword(password);

        // Assert
        _mockPasswordHasher.Verify(x => x.HashPassword(password), Times.Once);
    }

    [Theory]
    [InlineData("simple")]
    [InlineData("complex!@#$%")]
    [InlineData("Very Long Password With Spaces")]
    public void HashPassword_WithVariousPasswords_ShouldDelegateCorrectly(string password)
    {
        // Arrange
        _mockPasswordHasher.Setup(x => x.HashPassword(password)).Returns("hash");

        // Act
        _authenticationService.HashPassword(password);

        // Assert
        _mockPasswordHasher.Verify(x => x.HashPassword(password), Times.Once);
    }

    #endregion

    #region Integration Scenarios

    [Fact]
    public void Authenticate_CompleteSuccessFlow_ShouldWorkEndToEnd()
    {
        // Arrange
        var username = "john_doe";
        var password = "SecurePassword123!";
        var hashedPassword = "$2a$11$abcdefghijklmnopqrstuv";
        var expectedToken = "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...";

        var user = User.Create(1, username, hashedPassword, new List<Role> { Role.Admin });

        _mockUserRepository.Setup(x => x.GetByUsername(username)).Returns(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(password, hashedPassword)).Returns(true);
        _mockTokenService.Setup(x => x.GenerateToken(user)).Returns(expectedToken);

        // Act
        var result = _authenticationService.Authenticate(username, password);

        // Assert
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(expectedToken);
        result.Error.Should().BeEmpty();

        // Verify all interactions
        _mockUserRepository.Verify(x => x.GetByUsername(username), Times.Once);
        _mockPasswordHasher.Verify(x => x.VerifyPassword(password, hashedPassword), Times.Once);
        _mockTokenService.Verify(x => x.GenerateToken(user), Times.Once);
    }

    [Fact]
    public void Authenticate_CompleteFailureFlow_ShouldHandleGracefully()
    {
        // Arrange - Multiple failure scenarios
        var scenarios = new[]
        {
            new { Username = "", Password = "pass", ExpectedError = "Username is required" },
            new { Username = "user", Password = "", ExpectedError = "Password is required" },
            new { Username = "nonexistent", Password = "pass", ExpectedError = "Invalid username or password" }
        };

        // Setup for nonexistent user
        _mockUserRepository.Setup(x => x.GetByUsername("nonexistent")).Returns((User)null!);

        foreach (var scenario in scenarios)
        {
            // Act
            var result = _authenticationService.Authenticate(scenario.Username, scenario.Password);

            // Assert
            result.IsFailure.Should().BeTrue();
            result.Error.Should().Contain(scenario.ExpectedError);
        }
    }

    #endregion

    #region Mock Verification Tests

    [Fact]
    public void Authenticate_WhenUserNotFound_ShouldNotCallPasswordHasher()
    {
        // Arrange
        _mockUserRepository.Setup(x => x.GetByUsername(It.IsAny<string>())).Returns((User)null!);

        // Act
        _authenticationService.Authenticate("user", "pass");

        // Assert
        _mockPasswordHasher.Verify(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>()), Times.Never);
    }

    [Fact]
    public void Authenticate_WhenPasswordIncorrect_ShouldNotCallTokenService()
    {
        // Arrange
        var user = User.Create(1, "user", "hash", new List<Role> { Role.User });
        _mockUserRepository.Setup(x => x.GetByUsername(It.IsAny<string>())).Returns(user);
        _mockPasswordHasher.Setup(x => x.VerifyPassword(It.IsAny<string>(), It.IsAny<string>())).Returns(false);

        // Act
        _authenticationService.Authenticate("user", "pass");

        // Assert
        _mockTokenService.Verify(x => x.GenerateToken(It.IsAny<User>()), Times.Never);
    }

    #endregion
}
