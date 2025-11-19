// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Identity;

namespace TodoListManager.Infrastructure.Identity;

/// <summary>
/// Represents a user in the Identity system.
/// Extends IdentityUser to add custom properties if needed.
/// </summary>
public class ApplicationUser : IdentityUser<int>
{
    /// <summary>
    /// Date when the user was created.
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Optional: Additional custom properties can be added here.
    /// </summary>
}
