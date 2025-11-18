# Domain-Driven Design in TodoListManager

This document explains how **Domain-Driven Design (DDD)** principles and patterns are implemented in the TodoListManager backend application.

## Table of Contents

- [What is Domain-Driven Design?](#what-is-domain-driven-design)
- [Ubiquitous Language](#ubiquitous-language)
- [Bounded Contexts](#bounded-contexts)
- [Building Blocks](#building-blocks)
  - [Entities](#entities)
  - [Value Objects](#value-objects)
  - [Aggregates](#aggregates)
  - [Domain Services](#domain-services)
  - [Domain Events](#domain-events)
  - [Repositories](#repositories)
  - [Specifications](#specifications)
- [Business Rules in the Domain](#business-rules-in-the-domain)
- [Benefits](#benefits)

---

## What is Domain-Driven Design?

**Domain-Driven Design (DDD)** is a software development approach that focuses on:

1. **Understanding the business domain deeply**
2. **Modeling the core business logic in code**
3. **Creating a ubiquitous language** shared by developers and domain experts
4. **Protecting business rules** through proper encapsulation
5. **Separating domain logic** from infrastructure concerns

DDD is particularly valuable in complex domains where business rules are intricate and change frequently.

---

## Ubiquitous Language

The **ubiquitous language** is a shared vocabulary between developers and domain experts that is reflected directly in the code.

### Terms in Our Domain

| Term | Definition | Code Representation |
|------|------------|---------------------|
| **TodoList** | A collection of todo items with business rules | `TodoList` aggregate |
| **TodoItem** | A task with title, description, category, and progression tracking | `TodoItem` entity |
| **Progression** | A percentage-based progress entry at a specific date | `Progression` value object |
| **Category** | A classification for organizing todo items | `string` validated by `ICategoryValidator` |
| **User** | A person who can authenticate and manage todo lists | `User` aggregate |
| **Role** | A set of permissions assigned to users | `Role` entity |
| **Permission** | An authorization to perform specific actions | `Permission` entity |

### Language in Action

**Domain Expert:** "A todo item cannot be modified or removed once it reaches more than 50% progress."

**Code Representation:**

```csharp
// File: src/TodoListManager.Domain/Aggregates/TodoList.cs
public void UpdateItem(int id, string description)
{
    var item = GetItemOrThrow(id);

    // Business rule: Cannot modify items with more than 50% progress
    if (item.GetTotalProgress() > 50m)
    {
        throw new TodoItemCannotBeModifiedException(id);
    }

    item.UpdateDescription(description);
}
```

The code speaks the same language as the business requirement!

---

## Bounded Contexts

**Bounded contexts** define clear boundaries where a particular model applies. Our application has two primary bounded contexts:

### 1. Todo Management Context

**Responsibility:** Managing todo items, categories, and progression tracking

**Key Components:**
- `TodoList` aggregate
- `TodoItem` entity
- `Progression` value object
- `ITodoListRepository`
- `ICategoryValidator`

**Business Rules:**
- Todo items must have a valid category
- Progression percentages must be between 0 and 100
- Total progress cannot exceed 100%
- Items with >50% progress cannot be modified
- Progression dates must be chronological

### 2. Identity and Access Management Context

**Responsibility:** User authentication, authorization, and role management

**Key Components:**
- `User` aggregate
- `Role` entity
- `Permission` entity
- `Username` and `HashedPassword` value objects
- `IUserRepository`
- `IAuthenticationService`
- `ITokenService`

**Business Rules:**
- Users must have at least one role
- Usernames must be unique
- Passwords must be hashed
- Users can have multiple roles with combined permissions

---

## Building Blocks

DDD provides several tactical patterns (building blocks) for modeling the domain. Let's explore how each is implemented in our project.

---

### Entities

**Entities** are objects with a **unique identity** that persists over time, even if their attributes change.

#### Example 1: TodoItem Entity

**File:** `src/TodoListManager.Domain/Entities/TodoItem.cs`

```csharp
/// <summary>
/// Represents a todo item with progression tracking capabilities.
/// </summary>
public class TodoItem
{
    public int Id { get; private set; } // ← Identity
    public string Title { get; private set; }
    public string Description { get; private set; }
    public string Category { get; private set; }
    private readonly List<Progression> _progressions;
    public IReadOnlyList<Progression> Progressions => _progressions.AsReadOnly();

    public bool IsCompleted => GetTotalProgress() >= 100m;

    public TodoItem(int id, string title, string description, string category)
    {
        Id = id;
        Title = title;
        Description = description;
        Category = category;
        _progressions = new List<Progression>();
    }

    public void UpdateDescription(string description)
    {
        Description = description;
    }

    public void AddProgression(DateTime date, decimal percent)
    {
        _progressions.Add(new Progression(date, percent));
    }

    public decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }

    public DateTime? GetLastProgressionDate()
    {
        return _progressions.Count > 0 
            ? _progressions.Max(p => p.Date) 
            : null;
    }
}
```

**Key Entity Characteristics:**
- ✅ Has unique identity (`Id`)
- ✅ Identity doesn't change over lifetime
- ✅ Two TodoItems with same Id are considered equal, even if other properties differ
- ✅ Encapsulates business behavior (`AddProgression`, `GetTotalProgress`)
- ✅ Protects invariants (progressions cannot be modified directly)

#### Example 2: User Entity (Aggregate Root)

**File:** `src/TodoListManager.Domain/Entities/User.cs`

```csharp
/// <summary>
/// User aggregate root - manages user identity, authentication, and authorization.
/// </summary>
public class User
{
    public int Id { get; private set; } // ← Identity
    public Username Username { get; private set; }
    public HashedPassword PasswordHash { get; private set; }
    public List<Role> Roles { get; private set; }
    public DateTime CreatedAt { get; private set; }

    private User(int id, Username username, HashedPassword passwordHash, List<Role> roles)
    {
        if (id <= 0)
            throw new ArgumentException("User ID must be positive.", nameof(id));

        if (roles == null || !roles.Any())
            throw new ArgumentException("User must have at least one role.", nameof(roles));

        Id = id;
        Username = username ?? throw new ArgumentNullException(nameof(username));
        PasswordHash = passwordHash ?? throw new ArgumentNullException(nameof(passwordHash));
        Roles = roles;
        CreatedAt = DateTime.UtcNow;
    }

    /// <summary>
    /// Factory method to create a new User.
    /// </summary>
    public static User Create(int id, string username, string hashedPassword, List<Role> roles)
    {
        var usernameVo = Username.Create(username);
        var passwordVo = HashedPassword.FromHash(hashedPassword);

        return new User(id, usernameVo, passwordVo, roles);
    }

    public bool HasPermission(string permission)
    {
        if (string.IsNullOrWhiteSpace(permission))
            return false;

        return Roles.Any(role => role.Permissions.Any(p => p.Name == permission));
    }

    public bool HasRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            return false;

        return Roles.Any(role => role.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    }

    public void ChangePassword(HashedPassword newPasswordHash)
    {
        PasswordHash = newPasswordHash ?? throw new ArgumentNullException(nameof(newPasswordHash));
    }

    public void AddRole(Role role)
    {
        if (role == null)
            throw new ArgumentNullException(nameof(role));

        if (Roles.Any(r => r.Name.Equals(role.Name, StringComparison.OrdinalIgnoreCase)))
            throw new InvalidOperationException($"User already has role '{role.Name}'.");

        Roles.Add(role);
    }

    public void RemoveRole(string roleName)
    {
        if (string.IsNullOrWhiteSpace(roleName))
            throw new ArgumentException("Role name cannot be empty.", nameof(roleName));

        var role = Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
        if (role == null)
            throw new InvalidOperationException($"User does not have role '{roleName}'.");

        if (Roles.Count == 1)
            throw new InvalidOperationException("Cannot remove the last role. User must have at least one role.");

        Roles.Remove(role);
    }
}
```

**Key Entity Characteristics:**
- ✅ Has unique identity (`Id`)
- ✅ Uses factory method for creation (enforces invariants)
- ✅ Rich domain behavior (`HasPermission`, `AddRole`, `RemoveRole`)
- ✅ Protects business rules (e.g., must have at least one role)
- ✅ Uses value objects (`Username`, `HashedPassword`)

---

### Value Objects

**Value Objects** are objects without conceptual identity. They are defined by their attributes and are **immutable**.

#### Example 1: Progression Value Object

**File:** `src/TodoListManager.Domain/ValueObjects/Progression.cs`

```csharp
/// <summary>
/// Represents a progress entry for a todo item at a specific date.
/// </summary>
public class Progression
{
    public DateTime Date { get; private set; }
    public decimal Percent { get; private set; }

    public Progression(DateTime date, decimal percent)
    {
        Date = date;
        Percent = percent;
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Progression other)
            return false;

        return Date == other.Date && Percent == other.Percent;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Date, Percent);
    }
}
```

**Key Value Object Characteristics:**
- ✅ No identity - two progressions with same date and percent are considered equal
- ✅ Immutable - properties have `private set`
- ✅ Equality based on attributes (overridden `Equals` and `GetHashCode`)
- ✅ Represents a concept in the domain

#### Example 2: Username Value Object

**File:** `src/TodoListManager.Domain/ValueObjects/Username.cs`

```csharp
/// <summary>
/// Value object representing a username with validation.
/// </summary>
public class Username
{
    public string Value { get; private set; }

    private Username(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Factory method to create a Username with validation.
    /// </summary>
    public static Username Create(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            throw new ArgumentException("Username cannot be empty.", nameof(value));

        if (value.Length < 3)
            throw new ArgumentException("Username must be at least 3 characters long.", nameof(value));

        if (value.Length > 50)
            throw new ArgumentException("Username cannot exceed 50 characters.", nameof(value));

        return new Username(value);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not Username other)
            return false;

        return Value.Equals(other.Value, StringComparison.OrdinalIgnoreCase);
    }

    public override int GetHashCode()
    {
        return Value.ToLowerInvariant().GetHashCode();
    }

    public override string ToString()
    {
        return Value;
    }

    public static implicit operator string(Username username)
    {
        return username?.Value ?? string.Empty;
    }
}
```

**Key Value Object Characteristics:**
- ✅ Immutable
- ✅ Self-validating (validation in factory method)
- ✅ Equality based on value
- ✅ Encapsulates domain concept with business rules
- ✅ Prevents invalid usernames from existing

#### Example 3: HashedPassword Value Object

**File:** `src/TodoListManager.Domain/ValueObjects/HashedPassword.cs`

```csharp
/// <summary>
/// Value object representing a hashed password.
/// </summary>
public class HashedPassword
{
    public string Value { get; private set; }

    private HashedPassword(string value)
    {
        Value = value;
    }

    /// <summary>
    /// Creates a HashedPassword from an already hashed string.
    /// </summary>
    public static HashedPassword FromHash(string hashedValue)
    {
        if (string.IsNullOrWhiteSpace(hashedValue))
            throw new ArgumentException("Hashed password cannot be empty.", nameof(hashedValue));

        return new HashedPassword(hashedValue);
    }

    public override bool Equals(object? obj)
    {
        if (obj is not HashedPassword other)
            return false;

        return Value == other.Value;
    }

    public override int GetHashCode()
    {
        return Value.GetHashCode();
    }

    public override string ToString()
    {
        return "********"; // Never expose the hash
    }

    public static implicit operator string(HashedPassword password)
    {
        return password?.Value ?? string.Empty;
    }
}
```

**Key Value Object Characteristics:**
- ✅ Immutable
- ✅ Type safety (can't accidentally use plain string as password)
- ✅ Encapsulates security concern
- ✅ Hides hash in `ToString()` for security

---

### Aggregates

**Aggregates** are clusters of entities and value objects with a clear boundary. One entity is the **aggregate root**, which is the only entry point for modifications.

#### Example 1: TodoList Aggregate

**File:** `src/TodoListManager.Domain/Aggregates/TodoList.cs`

```csharp
/// <summary>
/// TodoList aggregate root - manages a collection of todo items with business rules.
/// </summary>
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items;
    private readonly ICategoryValidator _categoryValidator;

    public TodoList(ICategoryValidator categoryValidator)
    {
        _items = new Dictionary<int, TodoItem>();
        _categoryValidator = categoryValidator ?? throw new ArgumentNullException(nameof(categoryValidator));
    }

    /// <summary>
    /// Adds a new todo item to the list.
    /// </summary>
    public void AddItem(int id, string title, string description, string category)
    {
        // Domain validation through domain service
        if (!_categoryValidator.IsValidCategory(category))
        {
            throw new InvalidCategoryException(category);
        }

        var item = new TodoItem(id, title, description, category);
        _items[id] = item;
    }

    /// <summary>
    /// Updates the description of an existing todo item.
    /// </summary>
    public void UpdateItem(int id, string description)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Cannot modify items with more than 50% progress
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        item.UpdateDescription(description);
    }

    /// <summary>
    /// Removes a todo item from the list.
    /// </summary>
    public void RemoveItem(int id)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Cannot remove items with more than 50% progress
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }

        _items.Remove(id);
    }

    /// <summary>
    /// Registers a progression entry for a todo item with validation.
    /// </summary>
    public void RegisterProgression(int id, DateTime dateTime, decimal percent)
    {
        var item = GetItemOrThrow(id);

        // Business rule: Percent must be valid
        if (percent is <= 0 or >= 100)
        {
            throw new InvalidProgressionException("Percent must be greater than 0 and less than 100.");
        }

        // Business rule: Date must be greater than all existing progression dates
        var lastDate = item.GetLastProgressionDate();
        if (lastDate.HasValue && dateTime <= lastDate.Value)
        {
            throw new InvalidProgressionException("The progression date must be greater than all existing progression dates.");
        }

        // Business rule: Total progress cannot exceed 100%
        var currentTotal = item.GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new InvalidProgressionException($"Adding {percent}% would exceed 100% total progress. Current progress: {currentTotal}%");
        }

        item.AddProgression(dateTime, percent);
    }

    private TodoItem GetItemOrThrow(int id)
    {
        if (!_items.TryGetValue(id, out var item))
        {
            throw new TodoItemNotFoundException(id);
        }

        return item;
    }

    /// <summary>
    /// Gets all todo items sorted by ID.
    /// </summary>
    public IReadOnlyList<TodoItem> GetAllItems()
    {
        return _items.Values.OrderBy(i => i.Id).ToList();
    }
}
```

**Aggregate Characteristics:**
- ✅ **TodoList is the aggregate root** - all modifications go through it
- ✅ **Enforces invariants** - business rules are checked before modifications
- ✅ **Consistency boundary** - all changes are atomic and consistent
- ✅ **Encapsulation** - internal collection is private
- ✅ **Domain service injection** - uses `ICategoryValidator`
- ✅ **Clear interface** - `ITodoList` defines the aggregate's contract

**Why TodoList is an Aggregate:**
1. **Transaction boundary:** All items in a TodoList must be consistent
2. **Business rules span multiple entities:** Progression rules involve both TodoList and TodoItem
3. **Single entry point:** External code can't modify TodoItems directly
4. **Protects invariants:** Ensures business rules are always enforced

#### Example 2: User Aggregate

The `User` entity (shown earlier) is also an aggregate root:

**Aggregate Characteristics:**
- ✅ **User is the aggregate root** - contains Roles (entities) and value objects
- ✅ **Enforces invariants** - user must always have at least one role
- ✅ **Factory method** - `User.Create()` ensures valid creation
- ✅ **Business methods** - `AddRole()`, `RemoveRole()`, `HasPermission()`
- ✅ **Consistency boundary** - roles and permissions are managed together

---

### Domain Services

**Domain Services** contain domain logic that doesn't naturally fit into an entity or value object. They operate on domain objects.

#### Example 1: ICategoryValidator Domain Service

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

**Implementation:** `src/TodoListManager.Infrastructure/Services/CategoryValidator.cs`

```csharp
public class CategoryValidator : ICategoryValidator
{
    private readonly List<string> _validCategories = new()
    {
        "Work",
        "Personal",
        "Shopping",
        "Health",
        "Education",
        "Finance",
        "Home"
    };

    public bool IsValidCategory(string category)
    {
        if (string.IsNullOrWhiteSpace(category))
            return false;

        return _validCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }

    public IReadOnlyList<string> GetValidCategories()
    {
        return _validCategories.AsReadOnly();
    }
}
```

**Why it's a Domain Service:**
- ✅ Contains domain logic (category validation is a business rule)
- ✅ Doesn't belong to any specific entity
- ✅ Used by the domain layer (TodoList aggregate)
- ✅ Stateless operation

#### Example 2: IPasswordHasher Domain Service

**File:** `src/TodoListManager.Domain/Services/IPasswordHasher.cs`

```csharp
/// <summary>
/// Domain service for hashing and verifying passwords.
/// </summary>
public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}
```

**Why it's a Domain Service:**
- ✅ Password hashing is a domain concern (security is part of business requirements)
- ✅ Used by User aggregate and repository
- ✅ Doesn't belong to a specific entity
- ✅ Stateless operation

#### Example 3: IAuthenticationService Domain Service

**File:** `src/TodoListManager.Domain/Services/IAuthenticationService.cs`

```csharp
/// <summary>
/// Domain service for user authentication.
/// </summary>
public interface IAuthenticationService
{
    Result<string> Authenticate(string username, string password);
}
```

**Implementation:** `src/TodoListManager.Application/Services/AuthenticationService.cs`

```csharp
public class AuthenticationService : IAuthenticationService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthenticationService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public Result<string> Authenticate(string username, string password)
    {
        if (string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
            return Result<string>.Failure("Username and password are required.");

        var user = _userRepository.GetByUsername(username);
        if (user == null)
            return Result<string>.Failure("Invalid username or password.");

        if (!_passwordHasher.VerifyPassword(password, user.PasswordHash))
            return Result<string>.Failure("Invalid username or password.");

        var token = _tokenService.GenerateToken(user);
        return Result<string>.Success(token);
    }
}
```

**Why it's a Domain Service:**
- ✅ Orchestrates authentication logic (domain concern)
- ✅ Coordinates multiple aggregates and services
- ✅ Contains business logic (authentication rules)

---

### Domain Events

**Domain Events** represent something significant that happened in the domain.

#### Example 1: TodoItemCreatedEvent

**File:** `src/TodoListManager.Domain/Events/TodoItemCreatedEvent.cs`

```csharp
/// <summary>
/// Domain event raised when a new todo item is created.
/// </summary>
public class TodoItemCreatedEvent : IDomainEvent
{
    public int TodoItemId { get; }
    public string Title { get; }
    public string Category { get; }
    public DateTime OccurredOn { get; }

    public TodoItemCreatedEvent(int todoItemId, string title, string category)
    {
        TodoItemId = todoItemId;
        Title = title;
        Category = category;
        OccurredOn = DateTime.UtcNow;
    }
}
```

#### Example 2: ProgressionRegisteredEvent

**File:** `src/TodoListManager.Domain/Events/ProgressionRegisteredEvent.cs`

```csharp
/// <summary>
/// Domain event raised when progression is registered for a todo item.
/// </summary>
public class ProgressionRegisteredEvent : IDomainEvent
{
    public int TodoItemId { get; }
    public DateTime Date { get; }
    public decimal Percent { get; }
    public decimal TotalProgress { get; }
    public bool IsCompleted { get; }
    public DateTime OccurredOn { get; }

    public ProgressionRegisteredEvent(
        int todoItemId,
        DateTime date,
        decimal percent,
        decimal totalProgress,
        bool isCompleted)
    {
        TodoItemId = todoItemId;
        Date = date;
        Percent = percent;
        TotalProgress = totalProgress;
        IsCompleted = isCompleted;
        OccurredOn = DateTime.UtcNow;
    }
}
```

**Domain Events Characteristics:**
- ✅ Immutable (all properties are read-only)
- ✅ Represent past facts (named in past tense)
- ✅ Contain all relevant information
- ✅ Timestamp when event occurred
- ✅ Enable loose coupling between aggregates

**Use Cases for Domain Events:**
- Notification when item is completed
- Auditing and logging
- Triggering side effects (email, push notifications)
- Updating read models (CQRS)
- Cross-aggregate communication

---

### Repositories

**Repositories** provide the illusion of an in-memory collection of aggregates. They abstract persistence concerns from the domain.

#### Example 1: ITodoListRepository

**File:** `src/TodoListManager.Domain/Repositories/ITodoListRepository.cs`

```csharp
/// <summary>
/// Repository interface for TodoList aggregate persistence.
/// </summary>
public interface ITodoListRepository
{
    TodoList GetTodoList();
    void Save(TodoList todoList);
}
```

**Implementation:** `src/TodoListManager.Infrastructure/Repositories/InMemoryTodoListRepository.cs`

```csharp
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

**Repository Characteristics:**
- ✅ Interface defined in **Domain layer**
- ✅ Implementation in **Infrastructure layer**
- ✅ Works with aggregate roots only
- ✅ Provides collection-like interface
- ✅ Hides persistence details from domain

#### Example 2: IUserRepository

**File:** `src/TodoListManager.Domain/Repositories/IUserRepository.cs`

```csharp
/// <summary>
/// Repository interface for User aggregate persistence.
/// </summary>
public interface IUserRepository
{
    User? GetByUsername(string username);
    User? GetById(int id);
    void Add(User user);
    IEnumerable<User> GetAll();
}
```

**Repository Pattern Benefits:**
- ✅ Domain doesn't depend on persistence technology
- ✅ Easy to swap implementations (in-memory, SQL, NoSQL)
- ✅ Testability (can use fake repositories)
- ✅ Clear separation of concerns

---

### Specifications

**Specifications** encapsulate business rules and can be composed, reused, and tested independently.

#### Example 1: Base Specification

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
        return new NotSpecification<T>(this, other);
    }
}
```

#### Example 2: CanModifyTodoItemSpecification

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

    public string GetReason(TodoItem item)
    {
        if (item == null)
            return "Item is null";

        if (!IsSatisfiedBy(item))
            return $"Item cannot be modified because it has {item.GetTotalProgress()}% progress (maximum allowed: {MaxProgressThreshold}%)";

        return string.Empty;
    }
}
```

#### Example 3: ValidProgressionSpecification

**File:** `src/TodoListManager.Domain/Specifications/ValidProgressionSpecification.cs`

```csharp
/// <summary>
/// Specification to validate a progression entry.
/// </summary>
public class ValidProgressionSpecification : Specification<(decimal currentProgress, decimal newPercent)>
{
    public override bool IsSatisfiedBy((decimal currentProgress, decimal newPercent) candidate)
    {
        var (currentProgress, newPercent) = candidate;

        // Business rules
        if (newPercent <= 0 || newPercent >= 100)
            return false;

        if (currentProgress + newPercent > 100m)
            return false;

        return true;
    }
}
```

**Specification Pattern Benefits:**
- ✅ Business rules are explicit and testable
- ✅ Rules can be composed (`And`, `Or`, `Not`)
- ✅ Reusable across different contexts
- ✅ Domain logic is encapsulated
- ✅ Easier to maintain and evolve rules

---

## Business Rules in the Domain

One of the core principles of DDD is keeping business rules in the domain layer, not scattered across the application.

### Business Rules Implementation

#### Rule 1: Todo items with >50% progress cannot be modified

**Location:** `TodoList` aggregate

```csharp
public void UpdateItem(int id, string description)
{
    var item = GetItemOrThrow(id);

    // Business rule enforcement
    if (item.GetTotalProgress() > 50m)
    {
        throw new TodoItemCannotBeModifiedException(id);
    }

    item.UpdateDescription(description);
}
```

#### Rule 2: Total progress cannot exceed 100%

**Location:** `TodoList` aggregate

```csharp
public void RegisterProgression(int id, DateTime dateTime, decimal percent)
{
    var item = GetItemOrThrow(id);

    var currentTotal = item.GetTotalProgress();
    
    // Business rule enforcement
    if (currentTotal + percent > 100m)
    {
        throw new InvalidProgressionException(
            $"Adding {percent}% would exceed 100% total progress. Current progress: {currentTotal}%");
    }

    item.AddProgression(dateTime, percent);
}
```

#### Rule 3: Progression dates must be chronological

**Location:** `TodoList` aggregate

```csharp
public void RegisterProgression(int id, DateTime dateTime, decimal percent)
{
    var item = GetItemOrThrow(id);

    var lastDate = item.GetLastProgressionDate();
    
    // Business rule enforcement
    if (lastDate.HasValue && dateTime <= lastDate.Value)
    {
        throw new InvalidProgressionException(
            "The progression date must be greater than all existing progression dates.");
    }

    item.AddProgression(dateTime, percent);
}
```

#### Rule 4: User must always have at least one role

**Location:** `User` aggregate

```csharp
public void RemoveRole(string roleName)
{
    var role = Roles.FirstOrDefault(r => r.Name.Equals(roleName, StringComparison.OrdinalIgnoreCase));
    
    // Business rule enforcement
    if (Roles.Count == 1)
    {
        throw new InvalidOperationException(
            "Cannot remove the last role. User must have at least one role.");
    }

    Roles.Remove(role);
}
```

### Benefits of Domain-Centric Business Rules

✅ **Consistency:** Rules are enforced everywhere, not just in specific use cases  
✅ **Testability:** Business logic can be tested in isolation  
✅ **Discoverability:** Developers know where to find and add business rules  
✅ **Protection:** Invalid states are impossible to create  
✅ **Documentation:** Code expresses business requirements clearly

---

## Benefits

Implementing DDD in this project provides:

### 🎯 **Clear Business Logic**
- Business rules are explicit in the code
- Domain experts can read and validate the implementation
- Ubiquitous language reduces misunderstandings

### 🛡️ **Protected Invariants**
- Aggregates enforce consistency
- Invalid states cannot exist
- Business rules are always enforced

### 🔄 **Flexibility**
- Domain is independent of infrastructure
- Business logic can evolve without breaking infrastructure
- Easy to add new features within existing boundaries

### 🧪 **Testability**
- Domain logic can be tested without databases or frameworks
- Specifications are independently testable
- Aggregates have clear, testable behaviors

### 📈 **Scalability**
- Bounded contexts allow different parts to evolve independently
- Aggregates define natural transaction boundaries
- Domain events enable loose coupling

### 🎓 **Knowledge Transfer**
- Code reflects business requirements
- New developers can understand the domain through code
- Domain model serves as living documentation

---

## DDD Anti-Patterns (Avoided)

### ❌ **Anemic Domain Model** (What we avoided)

```csharp
// BAD: No behavior, just data
public class TodoItem
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string Description { get; set; }
    public List<Progression> Progressions { get; set; }
    // No methods, no business logic!
}

// Logic scattered in services
public class TodoItemService
{
    public void AddProgression(TodoItem item, Progression progression)
    {
        // Business logic outside the domain model!
        if (item.Progressions.Sum(p => p.Percent) + progression.Percent > 100)
            throw new Exception("Too much progress!");
            
        item.Progressions.Add(progression);
    }
}
```

### ✅ **Rich Domain Model** (What we did)

```csharp
// GOOD: Behavior-rich entity
public class TodoItem
{
    private readonly List<Progression> _progressions;
    
    public void AddProgression(DateTime date, decimal percent)
    {
        // Business logic in the domain model!
        _progressions.Add(new Progression(date, percent));
    }
    
    public decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }
}
```

---

## Conclusion

TodoListManager demonstrates **tactical DDD patterns** applied in a clean, maintainable way:

- **Entities** with identity and behavior
- **Value Objects** for immutable domain concepts
- **Aggregates** enforcing consistency boundaries
- **Domain Services** for operations spanning multiple entities
- **Repositories** abstracting persistence
- **Specifications** encapsulating business rules
- **Domain Events** for loose coupling

The domain layer is the **heart** of the application, containing all business logic and remaining independent of frameworks, databases, and external concerns.

---

**Related Documentation:**
- [Clean Architecture](CLEAN_ARCHITECTURE.md)
- [SOLID Principles](SOLID.md)
- [Setup Guide](SETUP.md)

[← Back to README](../README.md)
