// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentAssertions;
using TodoListManager.Domain.Entities;
using TodoListManager.Domain.ValueObjects;

namespace TodoListManager.Domain.Tests.Entities;

/// <summary>
/// Unit tests for User entity following best practices.
/// </summary>
public class UserTests
{
    #region Creation Tests

    [Fact]
    public void Create_WithValidParameters_ShouldCreateUser()
    {
        // Arrange
        var id = 1;
        var username = "testuser";
        var hashedPassword = "hashed_password_123";
        var roles = new List<Role> { Role.User };

        // Act
        var user = User.Create(id, username, hashedPassword, roles);

        // Assert
        user.Should().NotBeNull();
        user.Id.Should().Be(id);
        user.Username.Value.Should().Be(username);
        user.PasswordHash.Value.Should().Be(hashedPassword);
        user.Roles.Should().HaveCount(1);
        user.CreatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromSeconds(5));
    }

    [Fact]
    public void Create_WithMultipleRoles_ShouldSucceed()
    {
        // Arrange
        var roles = new List<Role> { Role.User, Role.Admin };

        // Act
        var user = User.Create(1, "testuser", "hash", roles);

        // Assert
        user.Roles.Should().HaveCount(2);
        user.Roles.Should().Contain(r => r.Name == "User");
        user.Roles.Should().Contain(r => r.Name == "Admin");
    }

    #endregion

    #region Validation Tests

    [Fact]
    public void Create_WithZeroId_ShouldThrowArgumentException()
    {
        // Arrange
        var roles = new List<Role> { Role.User };

        // Act
        Action act = () => User.Create(0, "testuser", "hash", roles);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID must be positive.*");
    }

    [Fact]
    public void Create_WithNegativeId_ShouldThrowArgumentException()
    {
        // Arrange
        var roles = new List<Role> { Role.User };

        // Act
        Action act = () => User.Create(-1, "testuser", "hash", roles);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User ID must be positive.*");
    }

    [Fact]
    public void Create_WithEmptyRolesList_ShouldThrowArgumentException()
    {
        // Arrange
        var emptyRoles = new List<Role>();

        // Act
        Action act = () => User.Create(1, "testuser", "hash", emptyRoles);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User must have at least one role.*");
    }

    [Fact]
    public void Create_WithNullRolesList_ShouldThrowArgumentException()
    {
        // Act
        Action act = () => User.Create(1, "testuser", "hash", null!);

        // Assert
        act.Should().Throw<ArgumentException>()
            .WithMessage("User must have at least one role.*");
    }

    [Theory]
    [InlineData("ab")]
    [InlineData("user@name")]
    [InlineData("")]
    public void Create_WithInvalidUsername_ShouldThrowArgumentException(string invalidUsername)
    {
        // Arrange
        var roles = new List<Role> { Role.User };

        // Act
        Action act = () => User.Create(1, invalidUsername, "hash", roles);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    [Fact]
    public void Create_WithEmptyHashedPassword_ShouldThrowArgumentException()
    {
        // Arrange
        var roles = new List<Role> { Role.User };

        // Act
        Action act = () => User.Create(1, "testuser", "", roles);

        // Assert
        act.Should().Throw<ArgumentException>();
    }

    #endregion

    #region HasPermission Tests

    [Fact]
    public void HasPermission_WithExistingPermission_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        var hasPermission = user.HasPermission("TodoList.Read");

        // Assert
        hasPermission.Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithNonExistingPermission_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        var hasPermission = user.HasPermission("TodoList.Delete");

        // Assert
        hasPermission.Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WithAdminRole_ShouldHaveAllPermissions()
    {
        // Arrange
        var user = User.Create(1, "admin", "hash", new List<Role> { Role.Admin });

        // Act & Assert
        user.HasPermission("TodoList.Read").Should().BeTrue();
        user.HasPermission("TodoList.Create").Should().BeTrue();
        user.HasPermission("TodoList.Update").Should().BeTrue();
        user.HasPermission("TodoList.Delete").Should().BeTrue();
        user.HasPermission("TodoList.Manage").Should().BeTrue();
    }

    [Fact]
    public void HasPermission_WithNullOrEmpty_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act & Assert
        user.HasPermission(null!).Should().BeFalse();
        user.HasPermission("").Should().BeFalse();
        user.HasPermission("   ").Should().BeFalse();
    }

    [Fact]
    public void HasPermission_WithMultipleRoles_ShouldCheckAllRoles()
    {
        // Arrange
        var customRole = new Role(3, "Custom", "Custom role", new List<Permission>
        {
            new Permission(10, "Custom.Permission", "Custom permission")
        });
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User, customRole });

        // Act & Assert
        user.HasPermission("TodoList.Read").Should().BeTrue();
        user.HasPermission("Custom.Permission").Should().BeTrue();
        user.HasPermission("TodoList.Delete").Should().BeFalse();
    }

    #endregion

    #region HasRole Tests

    [Fact]
    public void HasRole_WithExistingRole_ShouldReturnTrue()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        var hasRole = user.HasRole("User");

        // Assert
        hasRole.Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNonExistingRole_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        var hasRole = user.HasRole("Admin");

        // Assert
        hasRole.Should().BeFalse();
    }

    [Fact]
    public void HasRole_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act & Assert
        user.HasRole("user").Should().BeTrue();
        user.HasRole("USER").Should().BeTrue();
        user.HasRole("UsEr").Should().BeTrue();
    }

    [Fact]
    public void HasRole_WithNullOrEmpty_ShouldReturnFalse()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act & Assert
        user.HasRole(null!).Should().BeFalse();
        user.HasRole("").Should().BeFalse();
        user.HasRole("   ").Should().BeFalse();
    }

    #endregion

    #region ChangePassword Tests

    [Fact]
    public void ChangePassword_WithValidPassword_ShouldUpdatePassword()
    {
        // Arrange
        var user = User.Create(1, "testuser", "old_hash", new List<Role> { Role.User });
        var newPassword = HashedPassword.FromHash("new_hash");

        // Act
        user.ChangePassword(newPassword);

        // Assert
        user.PasswordHash.Should().Be(newPassword);
    }

    [Fact]
    public void ChangePassword_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        Action act = () => user.ChangePassword(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void ChangePassword_MultipleTimes_ShouldKeepLastPassword()
    {
        // Arrange
        var user = User.Create(1, "testuser", "original_hash", new List<Role> { Role.User });

        // Act
        user.ChangePassword(HashedPassword.FromHash("first_change"));
        user.ChangePassword(HashedPassword.FromHash("second_change"));
        var finalPassword = HashedPassword.FromHash("final_hash");
        user.ChangePassword(finalPassword);

        // Assert
        user.PasswordHash.Should().Be(finalPassword);
    }

    #endregion

    #region AddRole Tests

    [Fact]
    public void AddRole_WithValidRole_ShouldAddRole()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });
        var initialRoleCount = user.Roles.Count;

        // Act
        user.AddRole(Role.Admin);

        // Assert
        user.Roles.Should().HaveCount(initialRoleCount + 1);
        user.Roles.Should().Contain(r => r.Name == "Admin");
    }

    [Fact]
    public void AddRole_WithNull_ShouldThrowArgumentNullException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        Action act = () => user.AddRole(null!);

        // Assert
        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void AddRole_WithDuplicateRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        Action act = () => user.AddRole(Role.User);

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*already has role*");
    }

    #endregion

    #region RemoveRole Tests

    [Fact]
    public void RemoveRole_WithExistingRole_ShouldRemoveRole()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User, Role.Admin });

        // Act
        user.RemoveRole("User");

        // Assert
        user.Roles.Should().HaveCount(1);
        user.Roles.Should().NotContain(r => r.Name == "User");
    }

    [Fact]
    public void RemoveRole_ShouldBeCaseInsensitive()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User, Role.Admin });

        // Act
        user.RemoveRole("user");

        // Assert
        user.Roles.Should().NotContain(r => r.Name == "User");
    }

    [Fact]
    public void RemoveRole_WithNonExistingRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        Action act = () => user.RemoveRole("Admin");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*does not have role*");
    }

    [Fact]
    public void RemoveRole_LastRole_ShouldThrowInvalidOperationException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act
        Action act = () => user.RemoveRole("User");

        // Assert
        act.Should().Throw<InvalidOperationException>()
            .WithMessage("*Cannot remove the last role*");
    }

    [Fact]
    public void RemoveRole_WithNullOrEmpty_ShouldThrowArgumentException()
    {
        // Arrange
        var user = User.Create(1, "testuser", "hash", new List<Role> { Role.User });

        // Act & Assert
        Action actNull = () => user.RemoveRole(null!);
        actNull.Should().Throw<ArgumentException>()
            .WithMessage("Role name cannot be empty.*");

        Action actEmpty = () => user.RemoveRole("");
        actEmpty.Should().Throw<ArgumentException>()
            .WithMessage("Role name cannot be empty.*");
    }

    #endregion

    #region Integration Tests

    [Fact]
    public void User_CompleteWorkflow_ShouldWorkCorrectly()
    {
        // Arrange & Act - Create user
        var user = User.Create(1, "john_doe", "initial_hash", new List<Role> { Role.User });

        // Assert - Initial state
        user.HasRole("User").Should().BeTrue();
        user.HasPermission("TodoList.Read").Should().BeTrue();
        user.HasPermission("TodoList.Delete").Should().BeFalse();

        // Act - Add admin role
        user.AddRole(Role.Admin);

        // Assert - After adding admin role
        user.HasRole("Admin").Should().BeTrue();
        user.HasPermission("TodoList.Delete").Should().BeTrue();

        // Act - Change password
        user.ChangePassword(HashedPassword.FromHash("new_secure_hash"));

        // Assert - Password changed
        user.PasswordHash.Value.Should().Be("new_secure_hash");

        // Act - Remove user role
        user.RemoveRole("User");

        // Assert - User role removed
        user.HasRole("User").Should().BeFalse();
        user.Roles.Should().HaveCount(1);
    }

    #endregion
}
