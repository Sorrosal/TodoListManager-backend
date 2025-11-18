// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using Asp.Versioning.ApiExplorer;

namespace TodoListManager.API.Extensions;

/// <summary>
/// Extension methods for middleware configuration.
/// </summary>
public static class MiddlewareExtensions
{
    /// <summary>
    /// Configures Swagger UI middleware.
    /// </summary>
    /// <param name="app">The application builder.</param>
    /// <returns>The application builder for chaining.</returns>
    public static IApplicationBuilder UseSwaggerConfiguration(this IApplicationBuilder app)
    {
        var environment = app.ApplicationServices.GetRequiredService<IWebHostEnvironment>();
        
        if (environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(options =>
            {
                var provider = app.ApplicationServices.GetRequiredService<IApiVersionDescriptionProvider>();
                
                // Build a swagger endpoint for each discovered API version
                foreach (var description in provider.ApiVersionDescriptions.OrderByDescending(x => x.ApiVersion))
                {
                    options.SwaggerEndpoint(
                        $"/swagger/{description.GroupName}/swagger.json",
                        description.GroupName.ToUpperInvariant());
                }
            });
        }

        return app;
    }
}
