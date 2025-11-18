// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Asp.Versioning;

namespace TodoListManager.API.Extensions;

/// <summary>
/// Extension methods for API versioning configuration.
/// </summary>
public static class ApiVersioningExtensions
{
    /// <summary>
    /// Adds API versioning services.
    /// </summary>
    /// <param name="services">The service collection.</param>
    /// <returns>The service collection for chaining.</returns>
    public static IServiceCollection AddApiVersioningConfiguration(this IServiceCollection services)
    {
        services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = new UrlSegmentApiVersionReader();
        })
        .AddMvc()
        .AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

        return services;
    }
}
