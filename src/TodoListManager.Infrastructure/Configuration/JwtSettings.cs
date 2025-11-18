// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.Infrastructure.Configuration;

/// <summary>
/// Configuration settings for JWT authentication.
/// </summary>
public class JwtSettings
{
    public string Secret { get; set; } = string.Empty;
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public int ExpirationInMinutes { get; set; } = 60;
}
