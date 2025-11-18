// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using TodoListManager.Application.Handlers;
using TodoListManager.Application.Services;
using TodoListManager.Application.Validators;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;
using TodoListManager.Infrastructure.Repositories;
using TodoListManager.Infrastructure.Services;

namespace TodoListManager.API.Extensions;

/// <summary>
/// Extension methods for service registration.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<AddTodoItemCommandValidator>();
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Infrastructure service registrations
        services.AddScoped<ITokenService, JwtTokenService>();
        
        return services;
    }

    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Domain service registrations
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ICategoryValidator, CategoryValidator>();
        
        // Repository registrations
        services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        // Aggregate registrations
        services.AddSingleton<TodoList>();
        services.AddSingleton<ITodoList>(sp => sp.GetRequiredService<TodoList>());
        
        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Register command handlers
        services.AddScoped<AddTodoItemCommandHandler>();
        services.AddScoped<UpdateTodoItemCommandHandler>();
        services.AddScoped<RemoveTodoItemCommandHandler>();
        services.AddScoped<RegisterProgressionCommandHandler>();

        // Register query handlers
        services.AddScoped<GetAllTodoItemsQueryHandler>();

        // Register application services
        services.AddScoped<TodoListService>();
        services.AddScoped<TodoListPresentationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
