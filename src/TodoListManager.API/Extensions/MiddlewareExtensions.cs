// Copyright (c) Sergio Sorrosal. All Rights Reserved.

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
                options.SwaggerEndpoint("/swagger/v1/swagger.json", "Todo List Manager API v1");
            });
        }

        return app;
    }
}
