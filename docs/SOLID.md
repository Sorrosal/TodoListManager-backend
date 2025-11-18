# SOLID Principles in TodoListManager

This document explains how each of the **SOLID principles** is applied throughout the TodoListManager backend application, with concrete examples from the codebase.

## Table of Contents

- [What are SOLID Principles?](#what-are-solid-principles)
- [S - Single Responsibility Principle](#s---single-responsibility-principle-srp)
- [O - Open/Closed Principle](#o---openclosed-principle-ocp)
- [L - Liskov Substitution Principle](#l---liskov-substitution-principle-lsp)
- [I - Interface Segregation Principle](#i---interface-segregation-principle-isp)
- [D - Dependency Inversion Principle](#d---dependency-inversion-principle-dip)
- [Benefits in This Project](#benefits-in-this-project)

---

## What are SOLID Principles?

**SOLID** is an acronym for five design principles that make software designs more understandable, flexible, and maintainable:

- **S**ingle Responsibility Principle
- **O**pen/Closed Principle
- **L**iskov Substitution Principle
- **I**nterface Segregation Principle
- **D**ependency Inversion Principle

These principles, when applied together, help create a robust, testable, and maintainable codebase.

---

## S - Single Responsibility Principle (SRP)

> **"A class should have one, and only one, reason to change."**

Each class in our system has a single, well-defined responsibility.

### ✅ Example 1: Command Handlers

**File:** `src/TodoListManager.Application/Handlers/RegisterProgressionCommandHandler.cs`

```csharp
/// <summary>
/// Handles the command to register a progression for a todo item.
/// </summary>
public class RegisterProgressionCommandHandler : IRequestHandler<RegisterProgressionCommand, Result>
{
    private readonly ITodoList _todoList;

    public RegisterProgressionCommandHandler(ITodoList todoList)
    {
        _todoList = todoList;
    }

    public Task<Result> Handle(RegisterProgressionCommand command, CancellationToken cancellationToken)
    {
        try
        {
            _todoList.RegisterProgression(command.Id, command.Date, command.Percent);
            return Task.FromResult(Result.Success());
        }
        catch (TodoItemNotFoundException ex)
        {
            return Task.FromResult(Result.Failure(ex.Message));
        }
        // ...other exception handling
    }
}
```

**Single Responsibility:** This handler has **one reason to change**: if the business logic for registering progression changes. It doesn't handle validation (done by validators), doesn't manage HTTP concerns (done by controllers), and doesn't implement domain logic (done by the aggregate).

### ✅ Example 2: Validation Separation

**File:** `src/TodoListManager.Application/Validators/RegisterProgressionCommandValidator.cs`

Validators have a single responsibility: validate input data.

```csharp
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

**Single Responsibility:** Only validates the structure and format of commands. Business rule validation (e.g., total progress not exceeding 100%) is handled in the domain layer.

### ✅ Example 3: Domain Services

**File:** `src/TodoListManager.Domain/Services/ICategoryValidator.cs`

```csharp
/// <summary>
/// Domain service for validating todo item categories.
/// </summary>
public interface ICategoryValidator
{
    bool IsValidCategory(string category);
    IReadOnlyList<string> GetValidCategories();
}
```

**Single Responsibility:** Only validates categories. Doesn't handle persistence, doesn't handle HTTP requests, doesn't manage aggregates.

### ✅ Example 4: Password Hashing Service

**File:** `src/TodoListManager.Infrastructure/Services/PasswordHasher.cs`

```csharp
/// <summary>
/// Service for hashing and verifying passwords using BCrypt.
/// </summary>
public class PasswordHasher : IPasswordHasher
{
    public string HashPassword(string password)
    {
        return BCrypt.Net.BCrypt.HashPassword(password);
    }

    public bool VerifyPassword(string password, string hashedPassword)
    {
        return BCrypt.Net.BCrypt.Verify(password, hashedPassword);
    }
}
```

**Single Responsibility:** Only handles password hashing and verification. Authentication logic, user management, and token generation are separate concerns.

---

## O - Open/Closed Principle (OCP)

> **"Software entities should be open for extension, but closed for modification."**

Our system allows new functionality to be added without modifying existing code.

### ✅ Example 1: MediatR Pipeline Behaviors

**File:** `src/TodoListManager.Application/Behaviors/ValidationBehavior.cs`

```csharp
/// <summary>
/// MediatR pipeline behavior that validates requests using FluentValidation.
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
        // Validation logic...
        
        if (failures.Count != 0)
        {
            var errors = string.Join("; ", failures.Select(e => e.ErrorMessage));
            return (TResponse)(object)Result.Failure(errors);
        }

        return await next();
    }
}
```

**Open/Closed:** We can add new behaviors (logging, caching, authorization) by creating new `IPipelineBehavior` implementations **without modifying** existing handlers or the ValidationBehavior itself.

### ✅ Example 2: Specification Pattern

**File:** `src/TodoListManager.Domain/Specifications/Specification.cs`

```csharp
/// <summary>
/// Base class for specifications (business rules).
/// </summary>
public abstract class Specification<T> : ISpecification<T>
{
    public abstract bool IsSatisfiedBy(T candidate);

    public ISpecification<T> And(ISpecification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }

    public ISpecification<T> Or(ISpecification<T> other)
    {
        return new OrSpecification<T>(this, other);
    }

    public ISpecification<T> Not()
    {
        return new NotSpecification<T>(this);
    }
}
```

**File:** `src/TodoListManager.Domain/Specifications/CanModifyTodoItemSpecification.cs`

```csharp
/// <summary>
/// Specification to determine if a TodoItem can be modified.
/// </summary>
public class CanModifyTodoItemSpecification : Specification<TodoItem>
{
    private const decimal MaxProgressThreshold = 50m;

    public override bool IsSatisfiedBy(TodoItem candidate)
    {
        if (candidate == null)
            throw new ArgumentNullException(nameof(candidate));

        return candidate.GetTotalProgress() <= MaxProgressThreshold;
    }
}
```

**Open/Closed:** New business rules can be added by creating new `Specification<T>` implementations. Existing specifications don't need modification. Specifications can be combined using `And()`, `Or()`, `Not()`.

### ✅ Example 3: Repository Pattern

**File:** `src/TodoListManager.Domain/Repositories/ITodoListRepository.cs`

```csharp
/// <summary>
/// Repository interface for TodoList persistence.
/// </summary>
public interface ITodoListRepository
{
    TodoList GetTodoList();
    void Save(TodoList todoList);
}
```

**Open/Closed:** We can switch from in-memory storage to SQL Server, MongoDB, or any other persistence mechanism by creating a new implementation of `ITodoListRepository` **without changing** any business logic or handlers.

**Current Implementation:** `InMemoryTodoListRepository`  
**Possible Extension:** `SqlServerTodoListRepository`, `MongoDbTodoListRepository`

---

## L - Liskov Substitution Principle (LSP)

> **"Derived classes must be substitutable for their base classes."**

Implementations can be swapped without breaking the application.

### ✅ Example 1: Repository Implementations

**Interface:** `src/TodoListManager.Domain/Repositories/IUserRepository.cs`

```csharp
public interface IUserRepository
{
    User? GetByUsername(string username);
    User? GetById(int id);
    void Add(User user);
    IEnumerable<User> GetAll();
}
```

**Implementation:** `src/TodoListManager.Infrastructure/Repositories/InMemoryUserRepository.cs`

```csharp
public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users;
    private readonly IPasswordHasher _passwordHasher;

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        _passwordHasher = passwordHasher;
        _users = new List<User>();
        SeedAdminUser();
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

**Liskov Substitution:** Any implementation of `IUserRepository` (in-memory, SQL, NoSQL) can be substituted without breaking the application. The authentication service doesn't care about the implementation details.

**Usage in Authentication:**

```csharp
public class AuthController : ControllerBase
{
    private readonly IUserRepository _userRepository; // Works with ANY implementation

    public AuthController(IUserRepository userRepository)
    {
        _userRepository = userRepository;
    }
}
```

### ✅ Example 2: Token Service Implementations

**Interface:** `src/TodoListManager.Domain/Services/ITokenService.cs`

```csharp
public interface ITokenService
{
    string GenerateToken(User user);
}
```

**Implementation:** `src/TodoListManager.Infrastructure/Services/JwtTokenService.cs`

```csharp
public class JwtTokenService : ITokenService
{
    private readonly JwtSettings _jwtSettings;

    public JwtTokenService(IOptions<JwtSettings> jwtSettings)
    {
        _jwtSettings = jwtSettings.Value;
    }

    public string GenerateToken(User user)
    {
        // JWT-specific implementation
        var tokenHandler = new JwtSecurityTokenHandler();
        var key = Encoding.UTF8.GetBytes(_jwtSettings.Secret);
        // ...token generation logic
    }
}
```

**Liskov Substitution:** We could replace JWT with OAuth2, SAML, or any other token mechanism by implementing `ITokenService` without changing the authentication service or controllers.

### ✅ Example 3: Command Handlers

All command handlers implement `IRequestHandler<TCommand, Result>` from MediatR:

```csharp
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    // Implementation
}

public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result>
{
    // Implementation
}

public class RegisterProgressionCommandHandler : IRequestHandler<RegisterProgressionCommand, Result>
{
    // Implementation
}
```

**Liskov Substitution:** MediatR can work with any handler implementation. The pipeline behaviors work uniformly across all handlers without knowing their specific types.

---

## I - Interface Segregation Principle (ISP)

> **"Clients should not be forced to depend on interfaces they don't use."**

Interfaces are small, focused, and client-specific.

### ✅ Example 1: Focused Domain Services

Instead of one large `IUserService`:

```csharp
// ❌ BAD: Fat interface
public interface IUserService
{
    User Authenticate(string username, string password);
    string GenerateToken(User user);
    string HashPassword(string password);
    bool VerifyPassword(string password, string hash);
    bool ValidateCategory(string category);
    // ...many more methods
}
```

We have **segregated interfaces**:

```csharp
// ✅ GOOD: Focused interfaces

/// <summary>
/// Service for password hashing operations.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

/// <summary>
/// Service for JWT token generation.
/// </summary>
public interface ITokenService
{
    string GenerateToken(User user);
}

/// <summary>
/// Service for user authentication.
/// </summary>
public interface IAuthenticationService
{
    Result<string> Authenticate(string username, string password);
}

/// <summary>
/// Service for category validation.
/// </summary>
public interface ICategoryValidator
{
    bool IsValidCategory(string category);
    IReadOnlyList<string> GetValidCategories();
}
```

**Interface Segregation:** Each client depends only on the methods it actually uses:
- `InMemoryUserRepository` uses `IPasswordHasher`
- `AuthController` uses `IAuthenticationService`
- `TodoList` aggregate uses `ICategoryValidator`
- `AuthenticationService` uses `ITokenService`

### ✅ Example 2: Repository Interfaces

**File:** `src/TodoListManager.Domain/Repositories/ITodoListRepository.cs`

```csharp
/// <summary>
/// Repository for TodoList aggregate persistence.
/// </summary>
public interface ITodoListRepository
{
    TodoList GetTodoList();
    void Save(TodoList todoList);
}
```

**File:** `src/TodoListManager.Domain/Repositories/IUserRepository.cs`

```csharp
/// <summary>
/// Repository for User aggregate persistence.
/// </summary>
public interface IUserRepository
{
    User? GetByUsername(string username);
    User? GetById(int id);
    void Add(User user);
    IEnumerable<User> GetAll();
}
```

**Interface Segregation:** Separate repositories for different aggregates. Handlers that work with TodoList don't need to know about user operations, and vice versa.

### ✅ Example 3: Aggregate Interfaces

**File:** `src/TodoListManager.Domain/Aggregates/ITodoList.cs`

```csharp
/// <summary>
/// Interface for the TodoList aggregate root.
/// </summary>
public interface ITodoList
{
    void AddItem(int id, string title, string description, string category);
    void UpdateItem(int id, string description);
    void RemoveItem(int id);
    void RegisterProgression(int id, DateTime dateTime, decimal percent);
    IReadOnlyList<TodoItem> GetAllItems();
}
```

**Interface Segregation:** The interface exposes only the essential operations. Implementation details like internal validation methods or private helpers are hidden.

---

## D - Dependency Inversion Principle (DIP)

> **"Depend on abstractions, not concretions. High-level modules should not depend on low-level modules."**

This is the cornerstone of our Clean Architecture implementation.

### ✅ Example 1: Layer Dependencies

**Dependency Flow:**

```
API Layer (Presentation)
    ↓ depends on
Application Layer (Use Cases)
    ↓ depends on
Domain Layer (Business Logic) ← ABSTRACTIONS
    ↑ implemented by
Infrastructure Layer (Technical Details)
```

**Key Point:** The Domain layer defines interfaces (abstractions), and the Infrastructure layer implements them. The Domain layer **never** depends on Infrastructure.

### ✅ Example 2: Command Handler Dependencies

**File:** `src/TodoListManager.Application/Handlers/AddTodoItemCommandHandler.cs`

```csharp
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoList _todoList; // ← Abstraction (interface)

    public AddTodoItemCommandHandler(ITodoList todoList)
    {
        _todoList = todoList; // Injected by DI container
    }

    public Task<Result> Handle(AddTodoItemCommand command, CancellationToken cancellationToken)
    {
        try
        {
            // Works with abstraction, doesn't know about concrete implementation
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

**Dependency Inversion:** The handler depends on `ITodoList` (abstraction), not `TodoList` (concrete class). The concrete implementation is injected at runtime.

### ✅ Example 3: Dependency Injection Configuration

**File:** `src/TodoListManager.API/Extensions/ServiceCollectionExtensions.cs`

```csharp
public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddDomain(this IServiceCollection services)
    {
        // Domain service registrations - implementations injected from infrastructure
        services.AddSingleton<IPasswordHasher, PasswordHasher>();
        services.AddSingleton<ICategoryValidator, CategoryValidator>();
        
        // Repository registrations - abstractions defined in domain, implementations in infrastructure
        services.AddSingleton<ITodoListRepository, InMemoryTodoListRepository>();
        services.AddSingleton<IUserRepository, InMemoryUserRepository>();
        
        // Aggregate registrations - concrete class and interface
        services.AddSingleton<TodoList>();
        services.AddSingleton<ITodoList>(sp => sp.GetRequiredService<TodoList>());
        
        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services)
    {
        // Infrastructure service registrations
        services.AddScoped<ITokenService, JwtTokenService>();
        
        return services;
    }
}
```

**Dependency Inversion:** 
- High-level modules (Application handlers) depend on abstractions (`ITodoList`, `IUserRepository`)
- Low-level modules (Infrastructure repositories) implement these abstractions
- The DI container wires everything together at runtime

### ✅ Example 4: Aggregate Dependencies

**File:** `src/TodoListManager.Domain/Aggregates/TodoList.cs`

```csharp
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ICategoryValidator _categoryValidator; // ← Abstraction

    public TodoList(ICategoryValidator categoryValidator)
    {
        _items = new Dictionary<int, TodoItem>();
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
    }

    public void AddItem(int id, string title, string description, string category)
    {
        // Domain validation through domain service abstraction
        if (!_categoryValidator.IsValidCategory(category))
        {
            throw new InvalidCategoryException(category);
        }

        var item = new TodoItem(id, title, description, category);
        _items[id] = item;
    }
}
```

**Dependency Inversion:** The aggregate depends on `ICategoryValidator` (abstraction), not `CategoryValidator` (concrete implementation). The concrete validator is injected and can be easily swapped or mocked for testing.

### ✅ Example 5: Controller Dependencies

**File:** `src/TodoListManager.API/Controllers/AuthController.cs`

```csharp
[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthenticationService _authenticationService; // ← Abstraction
    private readonly IUserRepository _userRepository;                // ← Abstraction

    public AuthController(
        IAuthenticationService authenticationService,
        IUserRepository userRepository)
    {
        _authenticationService = authenticationService;
        _userRepository = userRepository;
    }

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequest request)
    {
        // Works with abstractions
        var result = _authenticationService.Authenticate(request.Username, request.Password);
        
        if (result.IsFailure)
            return Unauthorized(new { error = result.Error });

        var user = _userRepository.GetByUsername(request.Username);
        // ...
    }
}
```

**Dependency Inversion:** The controller depends on abstractions. We can easily:
- Swap authentication mechanisms (OAuth2, SAML)
- Change storage (SQL Server, MongoDB)
- Mock dependencies for unit testing

---

## Benefits in This Project

Applying SOLID principles throughout this project provides:

### 🧪 **Testability**
- Dependencies are injected as interfaces, making unit testing easy
- Each class has a single responsibility, simplifying test scenarios
- Mocking is straightforward due to interface-based design

### 🔧 **Maintainability**
- Changes are localized due to single responsibility
- New features can be added without modifying existing code
- Clear separation of concerns makes code easier to understand

### 🔄 **Flexibility**
- Implementations can be swapped without breaking the system
- Business rules are encapsulated in specifications
- Infrastructure can evolve independently of business logic

### 📈 **Scalability**
- New features extend existing abstractions
- Additional behaviors can be added via pipeline
- Multiple implementations can coexist (e.g., different repositories per tenant)

### 🎯 **Clean Architecture Enablement**
- Dependency inversion allows proper layer separation
- Domain remains independent of frameworks and infrastructure
- Application layer orchestrates without infrastructure knowledge

---

## Examples of SOLID Violations (Avoided)

### ❌ **SRP Violation (What we avoided)**

```csharp
// BAD: Multiple responsibilities in one class
public class TodoListService
{
    public void AddItem(...) { /* business logic */ }
    public void ValidateItem(...) { /* validation */ }
    public void SaveToDatabase(...) { /* persistence */ }
    public void SendNotification(...) { /* external communication */ }
    public string HashPassword(...) { /* security */ }
}
```

### ✅ **SRP Applied (What we did)**

- `TodoList` aggregate: Business logic
- `AddTodoItemCommandValidator`: Validation
- `ITodoListRepository`: Persistence abstraction
- `INotificationService`: External communication (if needed)
- `IPasswordHasher`: Security operations

### ❌ **DIP Violation (What we avoided)**

```csharp
// BAD: Handler depends on concrete implementation
public class AddTodoItemCommandHandler
{
    private readonly TodoListSqlRepository _repository; // Concrete class!

    public AddTodoItemCommandHandler()
    {
        _repository = new TodoListSqlRepository(); // Direct instantiation!
    }
}
```

### ✅ **DIP Applied (What we did)**

```csharp
// GOOD: Handler depends on abstraction
public class AddTodoItemCommandHandler
{
    private readonly ITodoListRepository _repository; // Interface!

    public AddTodoItemCommandHandler(ITodoListRepository repository)
    {
        _repository = repository; // Injected by DI container!
    }
}
```

---

## Conclusion

The TodoListManager backend demonstrates how **SOLID principles** create a maintainable, testable, and flexible architecture:

1. **SRP**: Each class has one clear responsibility
2. **OCP**: System is extensible without modification
3. **LSP**: Implementations are interchangeable
4. **ISP**: Interfaces are focused and client-specific
5. **DIP**: High-level modules depend on abstractions

These principles, combined with **Clean Architecture** and **Domain-Driven Design**, result in a professional-grade application that can evolve with changing requirements.

---

**Related Documentation:**
- [Clean Architecture](CLEAN_ARCHITECTURE.md)
- [Domain-Driven Design](DDD.md)
- [Setup Guide](SETUP.md)

[← Back to README](../README.md)
