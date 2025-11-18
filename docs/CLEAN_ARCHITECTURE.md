# Clean Architecture in TodoListManager

This document explains how **Clean Architecture** principles are implemented in the TodoListManager backend application, with detailed examples showing the layer separation and dependency flow.

## Table of Contents

- [What is Clean Architecture?](#what-is-clean-architecture)
- [The Dependency Rule](#the-dependency-rule)
- [Architecture Layers](#architecture-layers)
  - [Domain Layer (Core)](#domain-layer-core)
  - [Application Layer (Use Cases)](#application-layer-use-cases)
  - [Infrastructure Layer (Technical Details)](#infrastructure-layer-technical-details)
  - [API Layer (Presentation)](#api-layer-presentation)
- [Dependency Flow](#dependency-flow)
- [CQRS Pattern](#cqrs-pattern)
- [Benefits](#benefits)
- [Real-World Example: Add Todo Item Flow](#real-world-example-add-todo-item-flow)

---

## What is Clean Architecture?

**Clean Architecture** (also known as Onion Architecture or Hexagonal Architecture) is an architectural pattern that emphasizes:

1. **Independence of Frameworks** - Business logic doesn't depend on external libraries
2. **Testability** - Business rules can be tested without UI, database, or external dependencies
3. **Independence of UI** - UI can change without changing business logic
4. **Independence of Database** - Can switch databases without affecting business logic
5. **Independence of External Services** - Business rules don't know about external systems

### The Core Principle

> **"Dependencies point inward. Nothing in an inner circle can know anything about something in an outer circle."**

```
┌────────────────────────────────────────────────┐
│          API Layer (Presentation)              │
│  Controllers, Middleware, HTTP Concerns        │
└────────────────┬───────────────────────────────┘
                 │ depends on ↓
┌────────────────┴───────────────────────────────┐
│          Application Layer (Use Cases)         │
│  Commands, Queries, Handlers, DTOs             │
└────────────────┬───────────────────────────────┘
                 │ depends on ↓
┌────────────────┴───────────────────────────────┐
│          Domain Layer (Business Logic)         │
│  Entities, Aggregates, Rules, Interfaces       │
└────────────────┬───────────────────────────────┘
                 ↑ implements
┌────────────────┴───────────────────────────────┐
│       Infrastructure Layer (Technical)         │
│  Repositories, External Services, Database     │
└────────────────────────────────────────────────┘
```

---

## The Dependency Rule

### ✅ Allowed Dependencies (Inward)

```
API Layer → Application Layer ✅
Application Layer → Domain Layer ✅
Infrastructure Layer → Domain Layer ✅ (implements interfaces)
```

### ❌ Forbidden Dependencies (Outward)

```
Domain Layer → Application Layer ❌
Domain Layer → Infrastructure Layer ❌
Domain Layer → API Layer ❌
Application Layer → Infrastructure Layer ❌ (except abstractions)
Application Layer → API Layer ❌
```

### How We Enforce This

**1. Project References**

```xml
<!-- TodoListManager.Domain.csproj -->
<!-- NO PROJECT REFERENCES - It's the core! -->

<!-- TodoListManager.Application.csproj -->
<ItemGroup>
  <ProjectReference Include="..\TodoListManager.Domain\TodoListManager.Domain.csproj" />
</ItemGroup>

<!-- TodoListManager.Infrastructure.csproj -->
<ItemGroup>
  <ProjectReference Include="..\TodoListManager.Domain\TodoListManager.Domain.csproj" />
</ItemGroup>

<!-- TodoListManager.API.csproj -->
<ItemGroup>
  <ProjectReference Include="..\TodoListManager.Application\TodoListManager.Application.csproj" />
  <ProjectReference Include="..\TodoListManager.Infrastructure\TodoListManager.Infrastructure.csproj" />
  <ProjectReference Include="..\TodoListManager.Domain\TodoListManager.Domain.csproj" />
</ItemGroup>
```

**2. Interface Definitions in Domain**

Domain layer defines what it needs (interfaces), Infrastructure layer provides implementations.

---

## Architecture Layers

---

## Domain Layer (Core)

**Location:** `src/TodoListManager.Domain/`

**Purpose:** Contains all business logic, business rules, and domain concepts. This is the **heart** of the application.

### Characteristics

✅ **No external dependencies** - Only depends on .NET base libraries  
✅ **Framework-independent** - No ASP.NET, Entity Framework, or infrastructure concerns  
✅ **Defines interfaces** - Other layers implement these interfaces  
✅ **Business rules** - All domain logic lives here  
✅ **Persistence ignorance** - Doesn't know about databases

### Structure

```
TodoListManager.Domain/
├── Aggregates/
│   ├── ITodoList.cs               # Aggregate interface
│   └── TodoList.cs                # Aggregate implementation
│
├── Entities/
│   ├── TodoItem.cs                # Domain entity
│   ├── User.cs                    # Domain entity (aggregate root)
│   ├── Role.cs                    # Domain entity
│   └── Permission.cs              # Domain entity
│
├── ValueObjects/
│   ├── Progression.cs             # Value object
│   ├── Username.cs                # Value object
│   └── HashedPassword.cs          # Value object
│
├── Services/
│   ├── ICategoryValidator.cs      # Domain service interface
│   ├── IPasswordHasher.cs         # Domain service interface
│   ├── ITokenService.cs           # Domain service interface
│   └── IAuthenticationService.cs  # Domain service interface
│
├── Repositories/
│   ├── ITodoListRepository.cs     # Repository interface
│   └── IUserRepository.cs         # Repository interface
│
├── Specifications/
│   ├── ISpecification.cs          # Specification interface
│   ├── Specification.cs           # Base specification
│   ├── CanModifyTodoItemSpecification.cs
│   └── ValidProgressionSpecification.cs
│
├── Exceptions/
│   ├── DomainException.cs         # Base domain exception
│   ├── TodoItemNotFoundException.cs
│   ├── InvalidProgressionException.cs
│   └── TodoItemCannotBeModifiedException.cs
│
├── Events/
│   ├── IDomainEvent.cs            # Domain event interface
│   ├── TodoItemCreatedEvent.cs
│   ├── ProgressionRegisteredEvent.cs
│   └── UserLoggedInEvent.cs
│
└── Common/
    └── Result.cs                  # Result pattern for error handling
```

### Key Examples

#### Aggregate with Business Rules

**File:** `src/TodoListManager.Domain/Aggregates/TodoList.cs`

```csharp
/// <summary>
/// TodoList aggregate root - manages a collection of todo items with business rules.
/// NO DEPENDENCIES on infrastructure, database, or frameworks!
/// </summary>
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ICategoryValidator _categoryValidator; // Interface only!

    public TodoList(ICategoryValidator categoryValidator)
    {
        _items = new Dictionary<int, TodoItem>();
        _categoryValidator = categoryValidator;
    }

    public void RegisterProgression(int id, DateTime dateTime, decimal percent)
    {
        var item = GetItemOrThrow(id);

        // ✅ BUSINESS RULE: Percent must be valid
        if (percent is <= 0 or >= 100)
        {
            throw new InvalidProgressionException("Percent must be greater than 0 and less than 100.");
        }

        // ✅ BUSINESS RULE: Date must be chronological
        var lastDate = item.GetLastProgressionDate();
        if (lastDate.HasValue && dateTime <= lastDate.Value)
        {
            throw new InvalidProgressionException("The progression date must be greater than all existing progression dates.");
        }

        // ✅ BUSINESS RULE: Total progress cannot exceed 100%
        var currentTotal = item.GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new InvalidProgressionException($"Adding {percent}% would exceed 100% total progress. Current progress: {currentTotal}%");
        }

        item.AddProgression(dateTime, percent);
    }
}
```

**Why this belongs in Domain:**
- ✅ Pure business logic
- ✅ No HTTP, database, or framework concerns
- ✅ Testable without any infrastructure
- ✅ Implements business rules directly

#### Domain Service Interface

**File:** `src/TodoListManager.Domain/Services/IPasswordHasher.cs`

```csharp
/// <summary>
/// Domain service interface for password hashing.
/// Domain defines WHAT it needs, Infrastructure provides HOW.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

**Why it's an interface:**
- ✅ Domain defines what it needs
- ✅ Infrastructure provides implementation
- ✅ Dependency Inversion Principle
- ✅ Testable (can use mock implementation)

---

## Application Layer (Use Cases)

**Location:** `src/TodoListManager.Application/`

**Purpose:** Orchestrates the flow of data between the UI and the Domain. Contains **use cases** (application-specific business logic).

### Characteristics

✅ **Use case driven** - Each handler represents a use case  
✅ **Depends on Domain** - Uses domain entities and interfaces  
✅ **No infrastructure** - Doesn't depend on databases or frameworks  
✅ **Orchestration** - Coordinates domain objects and services  
✅ **DTOs** - Transforms data for external layers

### Structure

```
TodoListManager.Application/
├── Commands/
│   ├── AddTodoItemCommand.cs          # Command definition
│   ├── UpdateTodoItemCommand.cs
│   ├── RemoveTodoItemCommand.cs
│   └── RegisterProgressionCommand.cs
│
├── Queries/
│   └── GetAllTodoItemsQuery.cs        # Query definition
│
├── Handlers/
│   ├── AddTodoItemCommandHandler.cs   # Command handler
│   ├── UpdateTodoItemCommandHandler.cs
│   ├── RemoveTodoItemCommandHandler.cs
│   ├── RegisterProgressionCommandHandler.cs
│   └── GetAllTodoItemsQueryHandler.cs # Query handler
│
├── Validators/
│   ├── AddTodoItemCommandValidator.cs # FluentValidation validators
│   ├── UpdateTodoItemCommandValidator.cs
│   ├── RemoveTodoItemCommandValidator.cs
│   └── RegisterProgressionCommandValidator.cs
│
├── Behaviors/
│   └── ValidationBehavior.cs          # MediatR pipeline behavior
│
├── Services/
│   ├── AuthenticationService.cs       # Application service
│   └── TodoListPresentationService.cs
│
└── DTOs/
    ├── LoginRequest.cs                # Data Transfer Objects
    └── LoginResponse.cs
```

### Key Examples

#### Use Case: Register Progression (Command)

**File:** `src/TodoListManager.Application/Commands/RegisterProgressionCommand.cs`

```csharp
/// <summary>
/// Command to register a progression for a todo item.
/// Represents a USE CASE in the application.
/// </summary>
public class RegisterProgressionCommand : IRequest<Result>
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Percent { get; set; }
}
```

**File:** `src/TodoListManager.Application/Handlers/RegisterProgressionCommandHandler.cs`

```csharp
/// <summary>
/// Handler for registering progression.
/// ORCHESTRATES the use case but delegates business logic to the domain.
/// </summary>
public class RegisterProgressionCommandHandler : IRequestHandler<RegisterProgressionCommand, Result>
{
    private readonly ITodoList _todoList; // ← Domain interface!

    public RegisterProgressionCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    public Task<Result> Handle(RegisterProgressionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Delegates to domain - business logic lives there!
            _todoList.RegisterProgression(command.Id, command.Date, command.Percent);
            return Task.FromResult(Result.Success());
        }
        catch (TodoItemNotFoundException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        catch (InvalidProgressionException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        catch (DomainException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
    }
}
```

**Why this belongs in Application:**
- ✅ Orchestrates the use case
- ✅ Transforms exceptions into Result objects
- ✅ No business logic (delegated to domain)
- ✅ Depends on domain abstractions

#### Validation (Separated Concern)

**File:** `src/TodoListManager.Application/Validators/RegisterProgressionCommandValidator.cs`

```csharp
/// <summary>
/// Validates the structure and format of the command.
/// DOES NOT validate business rules (domain does that).
/// </summary>
public class RegisterProgressionCommandValidator : AbstractValidator<RegisterProgressionCommand>
{
    public RegisterProgressionCommandValidator()
    {
        RuleFor(x => x.Id)
            .GreaterThan(0)
            .WithMessage("Id must be greater than 0.");

        RuleFor(x => x.Date)
            .NotEmpty()
            .WithMessage("Date is required.");

        RuleFor(x => x.Percent)
            .GreaterThan(0)
            .WithMessage("Percent must be greater than 0.")
            .LessThan(100)
            .WithMessage("Percent must be less than 100.");
    }
}
```

**Validation Separation:**
- ✅ **Application Layer:** Structure validation (required fields, data types, ranges)
- ✅ **Domain Layer:** Business rule validation (total progress, chronological dates)

#### Pipeline Behavior (Cross-Cutting Concern)

**File:** `src/TodoListManager.Application/Behaviors/ValidationBehavior.cs`

```csharp
/// <summary>
/// MediatR pipeline behavior for automatic validation.
/// Applies to ALL commands and queries without modifying them.
/// </summary>
public class ValidationBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
    where TRequest : IRequest<TResponse>
    where TResponse : Result
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;

    public ValidationBehavior(IEnumerable<IValidator<TRequest>> validators)
    {
        _validators = validators;
    }

    public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
    {
        if (!_validators.Any())
        {
            return await next();
        }

        var context = new ValidationContext<TRequest>(request);

        var validationResults = await Task.WhenAll(
            _validators.Select(v => v.ValidateAsync(context, cancellationToken)));

        var failures = validationResults
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();

        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(e => e.ErrorMessage));
            return (TResponse)(object)Result.Failure(errors);
        }

        return await next();
    }
}
```

**Why this belongs in Application:**
- ✅ Cross-cutting concern (applies to all use cases)
- ✅ Infrastructure for request processing
- ✅ Doesn't contain business logic
- ✅ Applies validation before reaching handlers

---

## Infrastructure Layer (Technical Details)

**Location:** `src/TodoListManager.Infrastructure/`

**Purpose:** Provides concrete implementations of interfaces defined in the Domain layer. Contains all technical details (database, external services, etc.).

### Characteristics

✅ **Implements domain interfaces** - Provides concrete implementations  
✅ **Technical concerns** - Databases, file systems, external APIs, etc.  
✅ **Framework-specific** - Can use Entity Framework, Dapper, etc.  
✅ **Depends on Domain** - Implements domain interfaces  
✅ **Replaceable** - Can swap implementations without affecting domain

### Structure

```
TodoListManager.Infrastructure/
├── Repositories/
│   ├── InMemoryTodoListRepository.cs  # Repository implementation
│   └── InMemoryUserRepository.cs
│
├── Services/
│   ├── JwtTokenService.cs             # JWT-specific implementation
│   ├── PasswordHasher.cs              # BCrypt implementation
│   └── CategoryValidator.cs           # Category validation implementation
│
└── Configuration/
    └── JwtSettings.cs                 # Configuration class
```

### Key Examples

#### Repository Implementation

**File:** `src/TodoListManager.Infrastructure/Repositories/InMemoryTodoListRepository.cs`

```csharp
/// <summary>
/// In-memory implementation of ITodoListRepository.
/// IMPLEMENTS interface defined in Domain layer.
/// Could easily be swapped for SQL, MongoDB, etc.
/// </summary>
public class InMemoryTodoListRepository : ITodoListRepository
{
    private TodoList? _todoList;
    private readonly ICategoryValidator _categoryValidator;

    public InMemoryTodoListRepository(ICategoryValidator categoryValidator)
    {
        _categoryValidator = categoryValidator;
    }

    public TodoList GetTodoList()
    {
        _todoList ??= new TodoList(_categoryValidator);
        return _todoList;
    }

    public void Save(TodoList todoList)
    {
        _todoList = todoList ?? throw new ArgumentNullException(nameof(todoList));
    }
}
```

**Why this belongs in Infrastructure:**
- ✅ Technical implementation detail
- ✅ Could be replaced with SQL/NoSQL
- ✅ Depends on domain interface
- ✅ Domain doesn't know about this class

#### Domain Service Implementation

**File:** `src/TodoListManager.Infrastructure/Services/PasswordHasher.cs`

```csharp
/// <summary>
/// BCrypt implementation of IPasswordHasher.
/// IMPLEMENTS interface defined in Domain layer.
/// Domain doesn't know about BCrypt!
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        // ✅ BCrypt is an infrastructure detail
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        // ✅ BCrypt is an infrastructure detail
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
```

**Why this belongs in Infrastructure:**
- ✅ Uses external library (BCrypt)
- ✅ Technical implementation detail
- ✅ Could be replaced with Argon2, PBKDF2, etc.
- ✅ Domain only knows the interface

#### User Repository with Seeded Data

**File:** `src/TodoListManager.Infrastructure/Repositories/InMemoryUserRepository.cs`

```csharp
/// <summary>
/// In-memory user repository with seeded admin user.
/// Infrastructure concern: data storage and seeding.
/// </summary>
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly IPasswordHasher _passwordHasher;

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        _users = new List<User>();
        SeedAdminUser(); // ✅ Infrastructure concern
    }

    private void SeedAdminUser()
    {
        // ✅ Seeding is an infrastructure concern
        var hashedPassword = _passwordHasher.HashPassword("admin");

        var adminUser = User.Create(
            1,
            "admin",
            hashedPassword,
            new List<Role> { Role.Admin }
        );

        _users.Add(adminUser);
    }

    public User? GetByUsername(string username)
    {
        if (string.IsNullOrWhiteSpace(username))
            return null;

        return _users.FirstOrDefault(u =>
            u.Username.Value.Equals(username, StringComparison.OrdinalIgnoreCase));
    }

    // ...other methods
}
```

---

## API Layer (Presentation)

**Location:** `src/TodoListManager.API/`

**Purpose:** HTTP interface to the application. Handles HTTP requests, routes, authentication, API versioning, and Swagger documentation.

### Characteristics

✅ **HTTP concerns** - Controllers, routing, status codes  
✅ **Authentication/Authorization** - JWT, OAuth, etc.  
✅ **API versioning** - Version management  
✅ **Swagger/OpenAPI** - Documentation  
✅ **Dependency injection setup** - Wires everything together

### Structure

```
TodoListManager.API/
├── Controllers/
│   ├── TodoListController.cs      # HTTP endpoints for todo list
│   └── AuthController.cs          # HTTP endpoints for authentication
│
├── Extensions/
│   └── ServiceCollectionExtensions.cs  # DI registration
│
├── Configuration/
│   └── ConfigureSwaggerOptions.cs # Swagger configuration
│
└── Program.cs                     # Application entry point
```

### Key Examples

#### Controller (HTTP Interface)

**File:** `src/TodoListManager.API/Controllers/TodoListController.cs`

```csharp
/// <summary>
/// API controller for todo list operations.
/// ONLY HTTP CONCERNS - business logic is in Application/Domain layers.
/// </summary>
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
[Authorize] // ← HTTP concern
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator; // ← Application layer abstraction

    public TodoListController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Register progression for a todo item.
    /// </summary>
    [HttpPost("{id}/progression")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> RegisterProgression(int id, [FromBody] RegisterProgressionRequest request)
    {
        // ✅ HTTP concerns: routing, status codes, JSON binding
        var command = new RegisterProgressionCommand
        {
            Id = id,
            Date = request.Date,
            Percent = request.Percent
        };

        // ✅ Delegates to Application layer
        var result = await _mediator.Send(command);

        // ✅ HTTP concerns: status codes, response format
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });

        return Ok(new { message = "Progression registered successfully" });
    }

    // ...other endpoints
}
```

**Why this belongs in API:**
- ✅ HTTP-specific concerns (routing, status codes)
- ✅ Doesn't contain business logic
- ✅ Transforms HTTP requests to commands
- ✅ Transforms results to HTTP responses

#### Dependency Injection Setup

**File:** `src/TodoListManager.API/Extensions/ServiceCollectionExtensions.cs`

```csharp
/// <summary>
/// Extension methods for dependency injection registration.
/// This is where Clean Architecture layers are WIRED TOGETHER.
/// </summary>
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // ✅ Register domain services
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ICategoryValidator, CategoryValidator>();
        
        // ✅ Register repositories (interface → implementation)
        services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        // ✅ Register aggregates
        services.AddSingleton<TodoList>();
        services.AddSingleton<ITodoList>(sp => sp.GetRequiredService<TodoList>());
        
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // ✅ Register infrastructure services
        services.AddScoped<ITokenService, JwtTokenService>();
        
        return services;
    }

    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // ✅ Register MediatR (CQRS)
        services.AddMediatR(cfg =>
        {
            cfg.RegisterServicesFromAssembly(typeof(AddTodoItemCommand).Assembly);
        });

        // ✅ Register pipeline behaviors
        services.AddTransient(typeof(IPipelineBehavior<,>), typeof(ValidationBehavior<,>));

        // ✅ Register application services
        services.AddScoped<TodoListPresentationService>();
        services.AddScoped<IAuthenticationService, AuthenticationService>();

        return services;
    }
}
```

**Why this belongs in API:**
- ✅ Composition root (wires everything together)
- ✅ Knows about all layers
- ✅ Maps interfaces to implementations
- ✅ Infrastructure concern (DI is framework-specific)

#### Program.cs (Application Entry Point)

**File:** `src/TodoListManager.API/Program.cs`

```csharp
var builder = WebApplication.CreateBuilder(args);

// ✅ HTTP concerns
builder.Services.AddControllers();

// ✅ Infrastructure concerns
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("JwtSettings"));

// ✅ Authentication (HTTP/infrastructure concern)
var jwtSettings = builder.Configuration.GetSection("JwtSettings").Get<JwtSettings>();
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options => { /* ... */ });

builder.Services.AddAuthorization();

// ✅ API versioning (HTTP concern)
builder.Services.AddApiVersioning(options => { /* ... */ });

// ✅ Swagger (HTTP concern - documentation)
builder.Services.AddSwaggerGen();

// ✅ Clean Architecture layers wired together
builder.Services.AddValidation();
builder.Services.AddInfrastructure();
builder.Services.AddDomain();
builder.Services.AddApplication();

var app = builder.Build();

// ✅ HTTP pipeline configuration
app.UseSwagger();
app.UseSwaggerUI();
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

app.Run();
```

---

## Dependency Flow

### Visual Representation

```
┌─────────────────────────────────────────────────┐
│  API Layer                                      │
│  ┌──────────────────────────────────────────┐  │
│  │ TodoListController                       │  │
│  │  → depends on IMediator (Application)    │  │
│  └──────────────────────────────────────────┘  │
└──────────────────┬──────────────────────────────┘
                   │ depends on ↓
┌──────────────────┴──────────────────────────────┐
│  Application Layer                              │
│  ┌──────────────────────────────────────────┐  │
│  │ RegisterProgressionCommandHandler        │  │
│  │  → depends on ITodoList (Domain)         │  │
│  └──────────────────────────────────────────┘  │
└──────────────────┬──────────────────────────────┘
                   │ depends on ↓
┌──────────────────┴──────────────────────────────┐
│  Domain Layer (Core - No Dependencies)          │
│  ┌──────────────────────────────────────────┐  │
│  │ ITodoList interface                      │  │
│  │ TodoList aggregate                       │  │
│  │ Business rules                           │  │
│  └──────────────────────────────────────────┘  │
└──────────────────┬──────────────────────────────┘
                   ↑ implements
┌──────────────────┴──────────────────────────────┐
│  Infrastructure Layer                           │
│  ┌──────────────────────────────────────────┐  │
│  │ InMemoryTodoListRepository               │  │
│  │  → implements ITodoListRepository        │  │
│  └──────────────────────────────────────────┘  │
└─────────────────────────────────────────────────┘
```

### Real Dependency Example

**Domain defines interface:**
```csharp
// src/TodoListManager.Domain/Services/IPasswordHasher.cs
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

**Infrastructure implements:**
```csharp
// src/TodoListManager.Infrastructure/Services/PasswordHasher.cs
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }
    // ...
}
```

**Application uses:**
```csharp
// src/TodoListManager.Application/Services/AuthenticationService.cs
public class AuthenticationService : IAuthenticationService
{
    private readonly IPasswordHasher _passwordHasher; // ← Domain interface!

    public AuthenticationService(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher; // ← Infrastructure implementation injected
    }
}
```

**API wires it together:**
```csharp
// src/TodoListManager.API/Extensions/ServiceCollectionExtensions.cs
services.AddSingleton<IPasswordHasher, PasswordHasher>(); // Interface → Implementation
```

---

## CQRS Pattern

**CQRS (Command Query Responsibility Segregation)** separates read and write operations.

### Command (Write Operation)

**File:** `src/TodoListManager.Application/Commands/RegisterProgressionCommand.cs`

```csharp
/// <summary>
/// Command to modify state (write operation).
/// </summary>
public class RegisterProgressionCommand : IRequest<Result>
{
    public int Id { get; set; }
    public DateTime Date { get; set; }
    public decimal Percent { get; set; }
}
```

**Handler:**
```csharp
public class RegisterProgressionCommandHandler : IRequestHandler<RegisterProgressionCommand, Result>
{
    private readonly ITodoList _todoList;

    public Task<Result> Handle(RegisterProgressionCommand command, CancellationToken cancellationToken)
    {
        _todoList.RegisterProgression(command.Id, command.Date, command.Percent);
        return Task.FromResult(Result.Success());
    }
}
```

### Query (Read Operation)

**File:** `src/TodoListManager.Application/Queries/GetAllTodoItemsQuery.cs`

```csharp
/// <summary>
/// Query to read data (read operation).
/// </summary>
public class GetAllTodoItemsQuery : IRequest<Result<IReadOnlyList<TodoItem>>>
{
}
```

**Handler:**
```csharp
public class GetAllTodoItemsQueryHandler : IRequestHandler<GetAllTodoItemsQuery, Result<IReadOnlyList<TodoItem>>>
{
    private readonly ITodoList _todoList;

    public Task<Result<IReadOnlyList<TodoItem>>> Handle(GetAllTodoItemsQuery query, CancellationToken cancellationToken)
    {
        var items = _todoList.GetAllItems();
        return Task.FromResult(Result<IReadOnlyList<TodoItem>>.Success(items));
    }
}
```

### Benefits of CQRS

✅ **Clear separation** - Write and read operations are distinct  
✅ **Scalability** - Can optimize queries and commands independently  
✅ **Testability** - Each operation can be tested in isolation  
✅ **Flexibility** - Can use different data stores for reads and writes

---

## Benefits

Applying Clean Architecture provides:

### 🧪 **Testability**
- Domain logic can be tested without databases, HTTP, or frameworks
- Each layer can be tested independently
- Mocking is straightforward due to interface-based design

### 🔧 **Maintainability**
- Changes are localized to specific layers
- Clear separation of concerns
- Business logic is isolated and protected

### 🔄 **Flexibility**
- Can swap implementations without breaking business logic
- Infrastructure can evolve independently
- UI can change without affecting core logic

### 📈 **Scalability**
- Layers can scale independently
- CQRS enables separate scaling of reads and writes
- Clear boundaries enable distributed architectures

### 🎯 **Framework Independence**
- Core business logic doesn't depend on ASP.NET, EF, or any framework
- Can migrate to different frameworks without rewriting business logic
- Domain logic is portable across different applications

---

## Real-World Example: Add Todo Item Flow

Let's trace a complete request through all layers:

### 1. HTTP Request (API Layer)

```http
POST /api/v1/TodoList
Authorization: Bearer eyJhbGciOiJS...
Content-Type: application/json

{
  "id": 1,
  "title": "Learn Clean Architecture",
  "description": "Study layered architecture",
  "category": "Education"
}
```

### 2. Controller Receives Request (API Layer)

```csharp
// src/TodoListManager.API/Controllers/TodoListController.cs
[HttpPost]
public async Task<IActionResult> AddTodoItem([FromBody] AddTodoItemRequest request)
{
    // ✅ Transform HTTP request to Command
    var command = new AddTodoItemCommand
    {
        Id = request.Id,
        Title = request.Title,
        Description = request.Description,
        Category = request.Category
    };

    // ✅ Send to Application layer via MediatR
    var result = await _mediator.Send(command);

    // ✅ Transform result to HTTP response
    if (result.IsFailure)
        return BadRequest(new { error = result.Error });

    return Ok(new { message = "Todo item added successfully" });
}
```

### 3. Validation Pipeline (Application Layer)

```csharp
// src/TodoListManager.Application/Behaviors/ValidationBehavior.cs
public async Task<TResponse> Handle(TRequest request, RequestHandlerDelegate<TResponse> next, CancellationToken cancellationToken)
{
    // ✅ Validate command structure
    var validationResults = await Task.WhenAll(_validators.Select(v => v.ValidateAsync(context, cancellationToken)));
    
    if (failures.Count != 0)
    {
        return (TResponse)(object)Result.Failure(errors);
    }

    // ✅ Proceed to handler
    return await next();
}
```

### 4. Command Handler (Application Layer)

```csharp
// src/TodoListManager.Application/Handlers/AddTodoItemCommandHandler.cs
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoList _todoList; // ← Domain interface

    public Task<Result> Handle(AddTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // ✅ Delegate to domain
            _todoList.AddItem(command.Id, command.Title, command.Description, command.Category);
            return Task.FromResult(Result.Success());
        }
        catch (DomainException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
    }
}
```

### 5. Domain Business Logic (Domain Layer)

```csharp
// src/TodoListManager.Domain/Aggregates/TodoList.cs
public void AddItem(int id, string title, string description, string category)
{
    // ✅ Business rule: Category must be valid
    if (!_categoryValidator.IsValidCategory(category))
    {
        throw new InvalidCategoryException(category);
    }

    // ✅ Create entity
    var item = new TodoItem(id, title, description, category);
    _items[id] = item;
}
```

### 6. Category Validation (Infrastructure Layer)

```csharp
// src/TodoListManager.Infrastructure/Services/CategoryValidator.cs
public bool IsValidCategory(string category)
{
    return _validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
}
```

### Flow Summary

```
HTTP Request
    ↓
API Controller (transforms to Command)
    ↓
MediatR Pipeline (validation)
    ↓
Command Handler (orchestrates)
    ↓
Domain Aggregate (business logic)
    ↓
Domain Service (validation logic)
    ↓
Result flows back up the chain
    ↓
HTTP Response
```

---

## Conclusion

Clean Architecture in TodoListManager provides:

1. **Clear Layer Separation** - Each layer has a specific responsibility
2. **Dependency Rule** - Dependencies always point inward
3. **Framework Independence** - Business logic is isolated from technical concerns
4. **Testability** - Each layer can be tested independently
5. **Flexibility** - Easy to swap implementations and evolve the system

The architecture ensures that the **business logic (Domain)** remains the heart of the application, protected from external changes and framework churn.

---

**Related Documentation:**
- [Domain-Driven Design](DDD.md)
- [SOLID Principles](SOLID.md)
- [Setup Guide](SETUP.md)

[← Back to README](../README.md)
