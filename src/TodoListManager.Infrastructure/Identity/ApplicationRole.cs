// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Microsoft.AspNetCore.Identity;

namespace TodoListManager.Infrastructure.Identity;

/// <summary>
/// Represents a role in the Identity system.
/// Extends IdentityRole to add custom properties if needed.
/// </summary>
public class ApplicationRole : IdentityRole<int>
{
    /// <summary>
    /// Description of the role.
    /// </summary>
    public string? Description { get; set; }
}
