// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using TodoListManager.API.Configuration;

namespace TodoListManager.API.Extensions;

/// <summary>
/// Extension methods for Swagger/OpenAPI configuration.
/// </summary>
public static class SwaggerExtensions
{
    /// <summary>
    /// Adds Swagger generation services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddSwaggerConfiguration(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.ConfigureOptions<ConfigureSwaggerOptions>();
        services.AddSwaggerGen();

        return services;
    }
}
