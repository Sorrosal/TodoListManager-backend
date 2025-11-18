// Copyright (c) Sergio Sorrosal. All Rights Reserved.

namespace TodoListManager.API.Extensions;

/// <summary>
/// Extension methods for CORS configuration.
/// </summary>
public static class CorsExtensions
{
    /// <summary>
    /// Adds CORS policy that allows any origin, method, and header.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddCorsPolicy(this IServiceCollection services)
    {
        services.AddCors(options =>
        {
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
        });

        return services;
    }
}
