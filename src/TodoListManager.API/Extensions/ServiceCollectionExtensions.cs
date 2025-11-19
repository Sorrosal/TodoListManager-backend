// Copyright (c) Sergio Sorrosal. All Rights Reserved.

using FluentValidation;
using MediatR;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using TodoListManager.Application.Behaviors;
using TodoListManager.Application.Validators;
using TodoListManager.Domain.Aggregates;
using TodoListManager.Domain.Repositories;
using TodoListManager.Domain.Services;
using TodoListManager.Infrastructure.Data;
using TodoListManager.Infrastructure.Identity;
using TodoListManager.Infrastructure.Persistence;
using TodoListManager.Infrastructure.Services;

namespace TodoListManager.API.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssembly(typeof(AddTodoItemCommandValidator).Assembly);
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddDbContext<TodoDbContext>(options =>
        {
            var conn = configuration.GetConnectionString("DefaultConnection");
            options.UseSqlServer(conn);
        });

        services.AddIdentityCore<ApplicationUser>(options =>
        {
            options.Password.RequireDigit = false;
            options.Password.RequireLowercase = false;
            options.Password.RequireUppercase = false;
            options.Password.RequireNonAlphanumeric = false;
            options.Password.RequiredLength = 4;
            options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
            options.Lockout.MaxFailedAccessAttempts = 5;
            options.Lockout.AllowedForNewUsers = true;
            options.User.RequireUniqueEmail = true;
            options.SignIn.RequireConfirmedEmail = false;
        })
        .AddRoles<ApplicationRole>()
        .AddEntityFrameworkStores<TodoDbContext>()
        .AddDefaultTokenProviders()
        .AddSignInManager();

        services.AddScoped<ITodoListRepository, TodoListManager.Infrastructure.Repositories.EfTodoListRepository>();
        services.AddScoped(typeof(IGenericRepository<>), typeof(TodoListManager.Infrastructure.Repositories.GenericRepository<>));
        services.AddScoped<TodoListManager.Domain.Common.IUnitOfWork>(sp => sp.GetRequiredService<TodoDbContext>());

        return services;
    }

    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        services.AddSingleton<ICategoryValidator, CategoryValidator>();
        services.AddScoped<TodoList>();
        services.AddScoped<ITodoList>(sp => sp.GetRequiredService<TodoList>());
        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly));

        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));
        services.AddAutoMapper(typeof(TodoListManager.Application.Mappings.TodoItemMappingProfile).Assembly);
        services.AddScoped<IAuthenticationService, IdentityAuthenticationService>();

        return services;
    }

    public static async Task SeedDatabaseAsync(this WebApplication app)
    {
        using var scope = app.Services.CreateScope();
        var services = scope.ServiceProvider;
        
        try
        {
            var context = services.GetRequiredService<TodoDbContext>();
            var userManager = services.GetRequiredService<UserManager<ApplicationUser>>();
            var roleManager = services.GetRequiredService<RoleManager<ApplicationRole>>();
            
            await DbInitializer.SeedAsync(context, userManager, roleManager);
        }
        catch (Exception ex)
        {
            var logger = services.GetRequiredService<ILogger<Program>>();
            logger.LogError(ex, "An error occurred while seeding the database.");
        }
    }
}
