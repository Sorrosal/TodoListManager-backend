# 📚 Guía de Estudio - TodoListManager Backend

## Introducción

Esta guía te ayudará a entender y defender este proyecto, explicando los conceptos arquitectónicos clave, patrones de diseño y principios aplicados.

---

## 📋 Tabla de Contenidos

1. [Visión General del Proyecto](#visión-general-del-proyecto)
2. [Clean Architecture](#clean-architecture)
3. [Domain-Driven Design (DDD)](#domain-driven-design-ddd)
4. [Principios SOLID](#principios-solid)
5. [Patrones de Diseño](#patrones-de-diseño)
6. [Tecnologías Utilizadas](#tecnologías-utilizadas)
7. [Flujo de una Petición](#flujo-de-una-petición)
8. [Preguntas y Respuestas Comunes](#preguntas-y-respuestas-comunes)
9. [Conceptos Clave para Defender](#conceptos-clave-para-defender)

---

## Visión General del Proyecto

### ¿Qué es?
Una **API REST** para gestionar listas de tareas (TODO) con seguimiento de progreso porcentual, construida con **.NET 8**.

### Características Principales
- ✅ CRUD completo de tareas
- 📊 Seguimiento de progreso porcentual
- 🔐 Autenticación JWT con roles
- 🛡️ Reglas de negocio (ej: tareas con >50% progreso no se pueden modificar)
- ✔️ Validación con FluentValidation
- 🏗️ Arquitectura limpia y escalable

### Objetivo del Proyecto
Demostrar la aplicación práctica de **Clean Architecture**, **DDD** y **SOLID** en un proyecto real de .NET.

---

## Clean Architecture

### ¿Qué es Clean Architecture?

Clean Architecture es un patrón arquitectónico que organiza el código en **capas concéntricas** donde las dependencias apuntan hacia adentro.

```
┌─────────────────────────────────────┐
│          API Layer (UI)             │  ← Frameworks, Controllers
├─────────────────────────────────────┤
│     Application Layer (Casos Uso)   │  ← Handlers, Commands, Queries
├─────────────────────────────────────┤
│     Domain Layer (Lógica Negocio)   │  ← Entities, Aggregates, Rules
└─────────────────────────────────────┘
           ↑
┌─────────────────────────────────────┐
│   Infrastructure Layer (Detalles)   │  ← DB, External Services
└─────────────────────────────────────┘
```

### Regla de Dependencia
**"Las dependencias apuntan hacia adentro"**

- ✅ Domain **NO** depende de nadie
- ✅ Application depende de Domain
- ✅ Infrastructure depende de Domain
- ✅ API depende de Application, Infrastructure y Domain

### Las 4 Capas en el Proyecto

#### 1. **Domain Layer** (`TodoListManager.Domain`)
**Responsabilidad:** Lógica de negocio pura, reglas del dominio.

**Contenido:**
- `Aggregates/TodoList.cs` - Raíz de agregado que gestiona TodoItems
- `Entities/TodoItem.cs` - Entidad con identidad y comportamiento
- `ValueObjects/Progression.cs` - Objeto de valor inmutable
- `Services/ICategoryValidator.cs` - Servicio de dominio (interfaz)
- `Specifications/CanModifyTodoItemSpecification.cs` - Reglas de negocio encapsuladas
- `Repositories/ITodoListRepository.cs` - Abstracción para persistencia
- `Common/Result.cs` - Patrón Result para manejo de errores

**Características:**
- ❌ Sin dependencias externas (ni EF, ni ASP.NET)
- ✅ Solo lógica de negocio
- ✅ Testeable sin bases de datos

**Ejemplo de código:**
```csharp
// TodoList.cs - Aggregate Root
public class TodoList : ITodoList
{
    public void UpdateItem(int id, string description)
    {
        var item = GetItemOrThrow(id);
        
        // Regla de negocio: No modificar si >50% progreso
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }
        
        item.UpdateDescription(description);
    }
}
```

#### 2. **Application Layer** (`TodoListManager.Application`)
**Responsabilidad:** Casos de uso, orquestación de lógica de negocio.

**Contenido:**
- `Commands/AddTodoItemCommand.cs` - Comandos (escritura)
- `Queries/GetAllTodoItemsQuery.cs` - Consultas (lectura)
- `Handlers/AddTodoItemCommandHandler.cs` - Lógica de casos de uso
- `Validators/AddTodoItemCommandValidator.cs` - Validación con FluentValidation
- `Behaviors/ValidationBehavior.cs` - Pipeline de MediatR
- `DTOs/TodoItemDto.cs` - Objetos de transferencia de datos

**Patrón CQRS:**
```csharp
// Command (escritura)
public record AddTodoItemCommand(string Title, string Description, string Category) 
    : IRequest<Result>;

// Query (lectura)
public record GetAllTodoItemsQuery() 
    : IRequest<Result<GetAllTodoItemsResponse>>;
```

**Handler ejemplo:**
```csharp
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result> Handle(AddTodoItemCommand command, CancellationToken ct)
    {
        try
        {
            // 1. Obtener agregado
            var aggregate = await _repository.GetAggregateAsync(ct);
            
            // 2. Aplicar lógica de dominio
            aggregate.AddItem(0, command.Title, command.Description, command.Category);
            
            // 3. Persistir cambios
            var newItem = aggregate.GetAllItems().First(i => i.Id == 0);
            await _repository.SaveAsync(newItem, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
```

#### 3. **Infrastructure Layer** (`TodoListManager.Infrastructure`)
**Responsabilidad:** Implementación de detalles técnicos (DB, servicios externos).

**Contenido:**
- `Repositories/EfTodoListRepository.cs` - Implementación con EF Core
- `Services/JwtTokenService.cs` - Generación de tokens JWT
- `Services/CategoryValidator.cs` - Implementación de validador
- `Persistence/TodoDbContext.cs` - Contexto de Entity Framework
- `Identity/ApplicationUser.cs` - Usuario de ASP.NET Identity

**Ejemplo de Repository:**
```csharp
public class EfTodoListRepository : ITodoListRepository
{
    private readonly TodoDbContext _db;
    
    public async Task<TodoItem?> GetByIdAsync(int id, CancellationToken ct)
    {
        var entity = await _db.TodoItems
            .Include(t => t.Progressions)
            .FirstOrDefaultAsync(t => t.Id == id, ct);
            
        return entity == null ? null : MapToDomain(entity);
    }
    
    private TodoItem MapToDomain(TodoItemEntity entity)
    {
        var item = new TodoItem(entity.Id, entity.Title, 
            entity.Description, entity.Category);
            
        foreach (var p in entity.Progressions)
        {
            item.AddProgression(p.Date, p.Percent);
        }
        
        return item;
    }
}
```

#### 4. **API Layer** (`TodoListManager.API`)
**Responsabilidad:** Punto de entrada HTTP, configuración, middleware.

**Contenido:**
- `Controllers/TodoListController.cs` - Endpoints REST
- `Controllers/AuthController.cs` - Autenticación
- `Extensions/ServiceCollectionExtensions.cs` - Registro de DI
- `Program.cs` - Configuración de la aplicación

**Ejemplo de Controller:**
```csharp
[ApiController]
[Route("api/[controller]")]
[Authorize]
public class TodoListController : ControllerBase
{
    private readonly IMediator _mediator;
    
    [HttpPost("items")]
    [Authorize(Roles = "Admin,User")]
    public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken ct)
    {
        var command = new AddTodoItemCommand(request.Title, request.Description, request.Category);
        var result = await _mediator.Send(command, ct);
        
        if (result.IsFailure)
            return BadRequest(new { error = result.Error });
            
        return Ok(new { message = "Item added successfully" });
    }
}
```

### Beneficios de Clean Architecture

1. **Testabilidad** - Domain sin dependencias = fácil de testear
2. **Independencia de frameworks** - Puedes cambiar EF por Dapper sin tocar Domain
3. **Independencia de UI** - Puedes agregar GraphQL sin cambiar casos de uso
4. **Mantenibilidad** - Cambios localizados en una capa
5. **Regla del negocio protegida** - Domain es el corazón, inmutable a cambios externos

---

## Domain-Driven Design (DDD)

### ¿Qué es DDD?

DDD es un enfoque de diseño de software que se centra en:
1. Entender profundamente el **dominio del negocio**
2. Crear un **modelo de dominio** que refleje ese entendimiento
3. Usar un **lenguaje ubicuo** compartido por desarrolladores y expertos del negocio

### Bloques Tácticos de DDD

#### 1. **Entities (Entidades)**
Objetos con **identidad única** que persiste en el tiempo.

**Ejemplo: TodoItem**
```csharp
public class TodoItem : BaseEntity
{
    public int Id { get; private set; } // ← Identidad
    public string Title { get; private set; }
    private readonly List<Progression> _progressions;
    
    public TodoItem(int id, string title, string description, string category)
    {
        if (string.IsNullOrWhiteSpace(title))
            throw new ArgumentException("Title cannot be empty.");
            
        Id = id;
        Title = title;
        _progressions = new List<Progression>();
    }
    
    public void AddProgression(DateTime date, decimal percent)
    {
        _progressions.Add(new Progression(date, percent));
    }
    
    public decimal GetTotalProgress()
    {
        return _progressions.Sum(p => p.Percent);
    }
}
```

**Características:**
- ✅ Tiene identidad única (Id)
- ✅ Dos entidades con mismo Id son iguales aunque otros atributos difieran
- ✅ Encapsula comportamiento (AddProgression, GetTotalProgress)
- ✅ Protege invariantes (title no puede ser vacío)

#### 2. **Value Objects (Objetos de Valor)**
Objetos **sin identidad**, definidos por sus atributos, **inmutables**.

**Ejemplo: Progression**
```csharp
public sealed class Progression : IEquatable<Progression>
{
    public DateTime Date { get; }
    public decimal Percent { get; }
    
    public Progression(DateTime date, decimal percent)
    {
        if (percent < 0 || percent > 100)
            throw new ArgumentException("Percent must be between 0 and 100.");
            
        Date = date;
        Percent = percent;
    }
    
    // Igualdad basada en valores
    public bool Equals(Progression? other)
    {
        if (other is null) return false;
        return Date == other.Date && Percent == other.Percent;
    }
}
```

**Características:**
- ✅ Inmutable (propiedades solo con get)
- ✅ Sin identidad (dos objetos con mismos valores son iguales)
- ✅ Auto-validante (valida en constructor)
- ✅ Igualdad por valor, no por referencia

**Diferencia Entity vs Value Object:**
```csharp
// Entity: Identidad importa
var item1 = new TodoItem(1, "Task", "Description", "Work");
var item2 = new TodoItem(1, "Different Title", "Desc", "Work");
// item1 == item2 → TRUE (mismo Id)

// Value Object: Valores importan
var prog1 = new Progression(DateTime.Now, 50);
var prog2 = new Progression(DateTime.Now, 50);
// prog1 == prog2 → TRUE (mismos valores)
```

#### 3. **Aggregates (Agregados)**
Cluster de entidades y value objects con un **límite de consistencia**.

**Ejemplo: TodoList Aggregate**
```csharp
public class TodoList : ITodoList
{
    private readonly Dictionary<int, TodoItem> _items; // ← Colección interna privada
    private readonly ICategoryValidator _categoryValidator;
    
    public TodoList(ICategoryValidator categoryValidator)
    {
        _items = new Dictionary<int, TodoItem>();
        _categoryValidator = categoryValidator;
    }
    
    // Único punto de entrada para agregar items
    public void AddItem(int id, string title, string description, string category)
    {
        // Validación de regla de negocio
        if (!_categoryValidator.IsValidCategory(category))
        {
            throw new InvalidCategoryException(category);
        }
        
        var item = new TodoItem(id, title, description, category);
        _items[id] = item;
    }
    
    // Regla de negocio: No modificar si >50% progreso
    public void UpdateItem(int id, string description)
    {
        var item = GetItemOrThrow(id);
        
        if (item.GetTotalProgress() > 50m)
        {
            throw new TodoItemCannotBeModifiedException(id);
        }
        
        item.UpdateDescription(description);
    }
    
    // Regla de negocio compleja: Validar progresión
    public void RegisterProgression(int id, DateTime dateTime, decimal percent)
    {
        var item = GetItemOrThrow(id);
        
        // Regla 1: Porcentaje válido
        if (percent <= 0 || percent >= 100)
        {
            throw new InvalidProgressionException("Percent must be between 0 and 100.");
        }
        
        // Regla 2: Fecha cronológica
        var lastDate = item.GetLastProgressionDate();
        if (lastDate.HasValue && dateTime <= lastDate.Value)
        {
            throw new InvalidProgressionException("Date must be after all existing dates.");
        }
        
        // Regla 3: No exceder 100%
        var currentTotal = item.GetTotalProgress();
        if (currentTotal + percent > 100m)
        {
            throw new InvalidProgressionException($"Would exceed 100%. Current: {currentTotal}%");
        }
        
        item.AddProgression(dateTime, percent);
    }
    
    public IReadOnlyList<TodoItem> GetAllItems()
    {
        return _items.Values.OrderBy(i => i.Id).ToList();
    }
}
```

**Características del Aggregate:**
- ✅ **TodoList es Aggregate Root** - único punto de entrada
- ✅ **Límite de consistencia** - todas las reglas se validan aquí
- ✅ **Encapsulación** - colección `_items` es privada
- ✅ **Protege invariantes** - nadie puede modificar items directamente
- ✅ **Transacción** - cambios al aggregate son atómicos

**¿Por qué TodoList es un Aggregate?**
1. Las reglas de negocio involucran múltiples TodoItems
2. Necesitamos garantizar consistencia (ej: progreso total no excede 100%)
3. TodoItems no tienen sentido fuera del contexto de TodoList

#### 4. **Domain Services (Servicios de Dominio)**
Lógica de dominio que **no pertenece a ninguna entidad**.

**Ejemplo: CategoryValidator**
```csharp
// Interface en Domain
public interface ICategoryValidator
{
    bool IsValidCategory(string category);
    IReadOnlyCollection<string> GetValidCategories();
}

// Implementación en Infrastructure
public class CategoryValidator : ICategoryValidator
{
    private static readonly string[] ValidCategories = new[]
    {
        "Work", "Personal", "Education", "Health", "Finance", "Other"
    };
    
    public bool IsValidCategory(string category)
    {
        return ValidCategories.Contains(category, StringComparer.OrdinalIgnoreCase);
    }
    
    public IReadOnlyCollection<string> GetValidCategories()
    {
        return ValidCategories.ToList().AsReadOnly();
    }
}
```

**¿Cuándo usar Domain Services?**
- ❓ La operación no es responsabilidad natural de una entidad
- ❓ La operación involucra múltiples agregados
- ❓ Es lógica de negocio pura pero sin estado

**Ejemplos en el proyecto:**
- `ICategoryValidator` - Validar categorías
- `IPasswordHasher` - Hash de contraseñas
- `IAuthenticationService` - Autenticación de usuarios

#### 5. **Specifications (Especificaciones)**
Encapsulan **reglas de negocio** reutilizables.

**Ejemplo: CanModifyTodoItemSpecification**
```csharp
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
        if (!IsSatisfiedBy(item))
            return $"Item cannot be modified because it has {item.GetTotalProgress()}% progress";
            
        return string.Empty;
    }
}
```

**Uso:**
```csharp
var spec = new CanModifyTodoItemSpecification();

if (spec.IsSatisfiedBy(todoItem))
{
    // Permitir modificación
}
else
{
    // Mostrar razón: spec.GetReason(todoItem)
}
```

**Beneficios:**
- ✅ Regla de negocio explícita y nombrada
- ✅ Reutilizable en múltiples contextos
- ✅ Testeable independientemente
- ✅ Puede componerse (And, Or, Not)

#### 6. **Repositories (Repositorios)**
Abstracción para **persistir y recuperar agregados**.

**Interface (en Domain):**
```csharp
public interface ITodoListRepository
{
    Task<TodoItem?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<List<TodoItem>> GetAllDomainItemsAsync(CancellationToken cancellationToken = default);
    Task SaveAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task DeleteAsync(TodoItem item, CancellationToken cancellationToken = default);
    Task<TodoList> GetAggregateAsync(CancellationToken cancellationToken = default);
}
```

**Características:**
- ✅ Interface en **Domain**, implementación en **Infrastructure**
- ✅ Oculta detalles de persistencia
- ✅ Trabaja con **agregados**, no con tablas
- ✅ Proporciona "ilusión de colección en memoria"

---

## Principios SOLID

### S - Single Responsibility Principle (Responsabilidad Única)
**"Una clase debe tener una sola razón para cambiar"**

**Ejemplo en el proyecto:**
```csharp
// ✅ BUENO: Cada clase tiene una responsabilidad
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    // Solo maneja el caso de uso de agregar items
}

public class UpdateTodoItemCommandHandler : IRequestHandler<UpdateTodoItemCommand, Result>
{
    // Solo maneja el caso de uso de actualizar items
}

// ❌ MALO: Clase con múltiples responsabilidades
public class TodoItemService
{
    public void Add() { }
    public void Update() { }
    public void Delete() { }
    public void SendEmail() { } // ← Responsabilidad diferente!
}
```

### O - Open/Closed Principle (Abierto/Cerrado)
**"Abierto para extensión, cerrado para modificación"**

**Ejemplo en el proyecto:**
```csharp
// ✅ BUENO: Extensible mediante interfaces
public interface ITokenService
{
    string GenerateToken(ApplicationUser user);
}

// Puedes agregar nueva implementación sin modificar código existente
public class JwtTokenService : ITokenService { }
public class OAuthTokenService : ITokenService { } // ← Nueva implementación

// ✅ BUENO: Specifications son composables
var spec1 = new CanModifyTodoItemSpecification();
var spec2 = new HasValidCategorySpecification();
var combined = spec1.And(spec2); // ← Extensión sin modificación
```

### L - Liskov Substitution Principle (Sustitución de Liskov)
**"Los objetos de una superclase deben ser reemplazables por objetos de sus subclases"**

**Ejemplo en el proyecto:**
```csharp
// ✅ BUENO: Cualquier ISpecification<T> puede usarse igual
public interface ISpecification<T>
{
    bool IsSatisfiedBy(T candidate);
}

public class CanModifyTodoItemSpecification : ISpecification<TodoItem> { }
public class ValidProgressionSpecification : ISpecification<(decimal, decimal)> { }

// Ambas pueden usarse de la misma manera
bool result = specification.IsSatisfiedBy(candidate);
```

### I - Interface Segregation Principle (Segregación de Interfaces)
**"Los clientes no deberían depender de interfaces que no usan"**

**Ejemplo en el proyecto:**
```csharp
// ✅ BUENO: Interfaces específicas y focalizadas
public interface ICategoryValidator
{
    bool IsValidCategory(string category);
    IReadOnlyCollection<string> GetValidCategories();
}

public interface IPasswordHasher
{
    string HashPassword(string password);
    bool VerifyPassword(string password, string hashedPassword);
}

// ❌ MALO: Interface grande que hace muchas cosas
public interface ITodoService
{
    void Add();
    void Update();
    void Delete();
    void ValidateCategory();
    void HashPassword();
    void SendEmail();
}
```

### D - Dependency Inversion Principle (Inversión de Dependencias)
**"Depender de abstracciones, no de concreciones"**

**Ejemplo en el proyecto:**
```csharp
// ✅ BUENO: Handler depende de abstracción (ITodoListRepository)
public class AddTodoItemCommandHandler
{
    private readonly ITodoListRepository _repository; // ← Abstracción
    
    public AddTodoItemCommandHandler(ITodoListRepository repository)
    {
        _repository = repository;
    }
}

// La implementación concreta se registra en Infrastructure
services.AddScoped<ITodoListRepository, EfTodoListRepository>();

// ❌ MALO: Depender directamente de implementación
public class AddTodoItemCommandHandler
{
    private readonly EfTodoListRepository _repository; // ← Concreción!
}
```

---

## Patrones de Diseño

### 1. **CQRS (Command Query Responsibility Segregation)**

**Concepto:** Separar operaciones de **lectura** (Queries) de operaciones de **escritura** (Commands).

**En el proyecto:**
```csharp
// COMMAND - Modifica estado
public record AddTodoItemCommand(string Title, string Description, string Category) 
    : IRequest<Result>;

public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    public async Task<Result> Handle(AddTodoItemCommand command, CancellationToken ct)
    {
        // Lógica de escritura
        var aggregate = await _repository.GetAggregateAsync(ct);
        aggregate.AddItem(command.Title, command.Description, command.Category);
        await _unitOfWork.SaveChangesAsync(ct);
        return Result.Success();
    }
}

// QUERY - Solo lee datos
public record GetAllTodoItemsQuery() : IRequest<Result<GetAllTodoItemsResponse>>;

public class GetAllTodoItemsQueryHandler 
    : IRequestHandler<GetAllTodoItemsQuery, Result<GetAllTodoItemsResponse>>
{
    public async Task<Result<GetAllTodoItemsResponse>> Handle(GetAllTodoItemsQuery query, CancellationToken ct)
    {
        // Solo lectura, sin modificar estado
        var items = await _repository.GetAllDomainItemsAsync(ct);
        var dtos = items.Select(MapToDto).ToList();
        return Result.Success(new GetAllTodoItemsResponse { Items = dtos });
    }
}
```

**Beneficios:**
- ✅ Optimización independiente de lectura y escritura
- ✅ Modelos de lectura diferentes a modelos de escritura
- ✅ Escalabilidad (puedes tener DBs separadas)

### 2. **Repository Pattern**

**Concepto:** Abstracción sobre la capa de datos, proporciona "ilusión de colección en memoria".

```csharp
// Abstracción en Domain
public interface ITodoListRepository
{
    Task<TodoItem?> GetByIdAsync(int id);
    Task SaveAsync(TodoItem item);
}

// Implementación en Infrastructure
public class EfTodoListRepository : ITodoListRepository
{
    private readonly TodoDbContext _db;
    
    public async Task<TodoItem?> GetByIdAsync(int id)
    {
        // Mapeo de entidad EF a entidad de dominio
        var entity = await _db.TodoItems.FindAsync(id);
        return MapToDomain(entity);
    }
}
```

**Beneficios:**
- ✅ Domain no conoce EF Core
- ✅ Puedes cambiar de EF a Dapper sin tocar Domain
- ✅ Fácil de mockear en tests

### 3. **Result Pattern**

**Concepto:** Manejo explícito de errores sin excepciones.

```csharp
public class Result
{
    public bool IsSuccess { get; }
    public bool IsFailure => !IsSuccess;
    public string Error { get; }
    
    public static Result Success() => new Result(true, string.Empty);
    public static Result Failure(string error) => new Result(false, error);
}

public class Result<T> : Result
{
    public T Value { get; }
    public static Result<T> Success(T value) => new Result<T>(value, true, string.Empty);
}
```

**Uso:**
```csharp
// En Handler
public async Task<Result> Handle(AddTodoItemCommand command)
{
    if (!IsValid(command))
        return Result.Failure("Invalid command");
        
    // ... lógica ...
    
    return Result.Success();
}

// En Controller
var result = await _mediator.Send(command);

if (result.IsFailure)
    return BadRequest(new { error = result.Error });
    
return Ok(new { message = "Success" });
```

**Beneficios:**
- ✅ Errores explícitos (no ocultos en excepciones)
- ✅ Type-safe (compilador te obliga a manejar errores)
- ✅ Railway-oriented programming

### 4. **Specification Pattern**

**Concepto:** Encapsular reglas de negocio como objetos reutilizables.

```csharp
public abstract class Specification<T>
{
    public abstract bool IsSatisfiedBy(T candidate);
    
    public Specification<T> And(Specification<T> other)
    {
        return new AndSpecification<T>(this, other);
    }
}

public class CanModifyTodoItemSpecification : Specification<TodoItem>
{
    public override bool IsSatisfiedBy(TodoItem item)
    {
        return item.GetTotalProgress() <= 50m;
    }
}
```

### 5. **MediatR Pipeline Pattern**

**Concepto:** Comportamientos transversales aplicados a todos los requests.

```csharp
public class ValidationBehavior<TRequest, TResponse> 
    : IPipelineBehavior<TRequest, TResponse>
{
    private readonly IEnumerable<IValidator<TRequest>> _validators;
    
    public async Task<TResponse> Handle(
        TRequest request, 
        RequestHandlerDelegate<TResponse> next, 
        CancellationToken ct)
    {
        // 1. Validar antes de ejecutar handler
        var failures = _validators
            .Select(v => v.Validate(request))
            .SelectMany(r => r.Errors)
            .Where(f => f != null)
            .ToList();
            
        if (failures.Any())
        {
            var errors = string.Join("; ", failures.Select(e => e.ErrorMessage));
            return (TResponse)(object)Result.Failure(errors);
        }
        
        // 2. Ejecutar handler
        return await next();
    }
}
```

**Flujo de ejecución:**
```
Request → ValidationBehavior → Handler → Response
          ↑ Valida aquí
```

### 6. **Unit of Work Pattern**

**Concepto:** Gestionar transacciones y coordinar cambios.

```csharp
public interface IUnitOfWork
{
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}

// DbContext implementa IUnitOfWork
public class TodoDbContext : DbContext, IUnitOfWork
{
    public async Task<int> SaveChangesAsync(CancellationToken ct)
    {
        return await base.SaveChangesAsync(ct);
    }
}

// Uso en Handler
await _repository.SaveAsync(item, ct); // Marca cambios
await _unitOfWork.SaveChangesAsync(ct); // Persiste todo
```

---

## Tecnologías Utilizadas

### Backend Framework
- **.NET 8** - Framework principal
- **C# 12** - Lenguaje de programación

### Librerías Principales

#### **MediatR**
- Implementa patrón Mediator y CQRS
- Desacopla Controllers de Handlers

```csharp
// Controller envía comando
var result = await _mediator.Send(new AddTodoItemCommand(...));

// MediatR enruta a handler apropiado
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
```

#### **FluentValidation**
- Validación fluida y expresiva

```csharp
public class AddTodoItemCommandValidator : AbstractValidator<AddTodoItemCommand>
{
    public AddTodoItemCommandValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Title is required")
            .MaximumLength(200).WithMessage("Title too long");
            
        RuleFor(x => x.Category)
            .NotEmpty()
            .Must(BeValidCategory).WithMessage("Invalid category");
    }
}
```

#### **Entity Framework Core 8**
- ORM para acceso a datos
- Migraciones automáticas

```csharp
public class TodoDbContext : DbContext
{
    public DbSet<TodoItemEntity> TodoItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItemEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired();
            b.HasMany(x => x.Progressions)
                .WithOne(p => p.TodoItem)
                .HasForeignKey(p => p.TodoItemId);
        });
    }
}
```

#### **ASP.NET Core Identity**
- Gestión de usuarios y roles
- Autenticación y autorización

```csharp
services.AddIdentityCore<ApplicationUser>()
    .AddRoles<ApplicationRole>()
    .AddEntityFrameworkStores<TodoDbContext>();
```

#### **JWT (JSON Web Tokens)**
- Autenticación stateless

```csharp
[Authorize] // ← Requiere JWT válido
[Authorize(Roles = "Admin")] // ← Requiere rol Admin
```

#### **Swagger/OpenAPI**
- Documentación automática de API

### Testing

#### **xUnit**
- Framework de testing

#### **FluentAssertions**
- Assertions más legibles

```csharp
result.Should().BeOfType<Result>();
result.IsSuccess.Should().BeTrue();
item.Title.Should().Be("Expected Title");
```

#### **Moq**
- Mocking de dependencias

```csharp
var mockRepository = new Mock<ITodoListRepository>();
mockRepository
    .Setup(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()))
    .ReturnsAsync(todoItem);
```

#### **NetArchTest**
- Tests de arquitectura

```csharp
[Fact]
public void Domain_Should_NotDependOn_Infrastructure()
{
    var result = Types.InAssembly(domainAssembly)
        .Should()
        .NotHaveDependencyOn("Infrastructure")
        .GetResult();
        
    Assert.True(result.IsSuccessful);
}
```

---

## Flujo de una Petición

Veamos el flujo completo de **agregar un TodoItem**:

### 1. **Request HTTP**
```http
POST /api/TodoList/items
Authorization: Bearer eyJhbGc...
Content-Type: application/json

{
    "title": "Learn DDD",
    "description": "Study Domain-Driven Design",
    "category": "Education"
}
```

### 2. **Controller (API Layer)**
```csharp
[HttpPost("items")]
[Authorize(Roles = "Admin,User")]
public async Task<IActionResult> AddItem([FromBody] AddItemRequest request, CancellationToken ct)
{
    // 1. Mapear DTO a Command
    var command = new AddTodoItemCommand(request.Title, request.Description, request.Category);
    
    // 2. Enviar a MediatR
    var result = await _mediator.Send(command, ct);
    
    // 3. Retornar respuesta
    if (result.IsFailure)
        return BadRequest(new { error = result.Error });
        
    return Ok(new { message = "Item added successfully" });
}
```

### 3. **MediatR Pipeline**
```
Command → ValidationBehavior → Handler
```

**ValidationBehavior:**
```csharp
// Valida comando antes de llegar a handler
var failures = _validators
    .Select(v => v.Validate(command))
    .SelectMany(r => r.Errors)
    .ToList();
    
if (failures.Any())
    return Result.Failure(errors);
    
// Si válido, continúa a handler
return await next();
```

### 4. **Handler (Application Layer)**
```csharp
public class AddTodoItemCommandHandler : IRequestHandler<AddTodoItemCommand, Result>
{
    private readonly ITodoListRepository _repository;
    private readonly IUnitOfWork _unitOfWork;
    
    public async Task<Result> Handle(AddTodoItemCommand command, CancellationToken ct)
    {
        try
        {
            // 1. Obtener agregado del repositorio
            var aggregate = await _repository.GetAggregateAsync(ct);
            
            // 2. Aplicar lógica de dominio
            aggregate.AddItem(0, command.Title, command.Description, command.Category);
            
            // 3. Obtener item recién creado
            var newItem = aggregate.GetAllItems().First(i => i.Id == 0);
            
            // 4. Persistir cambios
            await _repository.SaveAsync(newItem, ct);
            await _unitOfWork.SaveChangesAsync(ct);
            
            return Result.Success();
        }
        catch (DomainException ex)
        {
            return Result.Failure(ex.Message);
        }
    }
}
```

### 5. **Aggregate (Domain Layer)**
```csharp
public class TodoList : ITodoList
{
    public void AddItem(int id, string title, string description, string category)
    {
        // Validar regla de negocio
        if (!_categoryValidator.IsValidCategory(category))
        {
            throw new InvalidCategoryException(category);
        }
        
        // Crear entidad
        var item = new TodoItem(id, title, description, category);
        
        // Agregar a colección interna
        _items[id] = item;
    }
}
```

### 6. **Repository (Infrastructure Layer)**
```csharp
public async Task SaveAsync(TodoItem item, CancellationToken ct)
{
    // Mapear entidad de dominio a entidad de EF
    var entity = new TodoItemEntity
    {
        Id = item.Id,
        Title = item.Title,
        Description = item.Description,
        Category = item.Category
    };
    
    // Guardar en DbContext
    await _db.TodoItems.AddAsync(entity, ct);
    await _db.SaveChangesAsync(ct);
}
```

### 7. **Response HTTP**
```http
HTTP/1.1 200 OK
Content-Type: application/json

{
    "message": "Item added successfully"
}
```

### Diagrama de Flujo
```
1. HTTP Request (JSON)
   ↓
2. Controller → Mapea a Command
   ↓
3. MediatR Pipeline
   ├─ ValidationBehavior (valida)
   └─ Handler (ejecuta)
   ↓
4. Handler → Obtiene Aggregate
   ↓
5. Aggregate → Aplica reglas de negocio
   ↓
6. Repository → Persiste en DB
   ↓
7. Unit of Work → Commit transacción
   ↓
8. Result → Vuelve a Controller
   ↓
9. HTTP Response (200 OK)
```

---

## Preguntas y Respuestas Comunes

### **¿Por qué Clean Architecture?**

**R:** Para conseguir:
1. **Testabilidad** - Domain sin dependencias se testea fácilmente
2. **Independencia de frameworks** - Puedes cambiar EF por otro ORM
3. **Mantenibilidad** - Cambios localizados en capas específicas
4. **Reglas de negocio protegidas** - Domain es inmutable a cambios externos

### **¿Por qué usar DDD?**

**R:** DDD es ideal cuando:
- El negocio tiene **reglas complejas** (ej: progreso no puede exceder 100%)
- Hay **lógica de validación** que involucra múltiples entidades
- Necesitas **comunicar** efectivamente con expertos del negocio
- Quieres código que **refleje el negocio** (código autodocumentado)

### **¿Qué es un Aggregate y por qué usarlos?**

**R:** Un aggregate es:
- **Límite de consistencia** - Todas las reglas se validan dentro
- **Límite transaccional** - Cambios son atómicos
- **Punto de entrada único** - Todo pasa por el aggregate root

**Ejemplo:** TodoList valida que ningún item exceda 50% antes de modificarlo.

### **¿Por qué Value Objects?**

**R:** Los Value Objects:
- Son **inmutables** - no se pueden modificar después de crear
- **Auto-validan** - garantizan que no pueden existir en estado inválido
- **Simplifican código** - encapsulan validación y comportamiento

**Ejemplo:** `Progression` no puede tener percent negativo o >100.

### **¿Cuándo usar Domain Service vs Entity?**

**R:** Usa Domain Service cuando:
- La operación **no pertenece naturalmente** a ninguna entidad
- Involucra **múltiples entidades**
- Es lógica de **negocio sin estado**

**Ejemplo:** `CategoryValidator` valida categorías, no pertenece a TodoItem.

### **¿Por qué CQRS?**

**R:** CQRS permite:
- **Optimización independiente** de lectura y escritura
- **Modelos diferentes** para lectura (DTOs) y escritura (Commands)
- **Escalabilidad** - puedes tener DBs separadas

### **¿Qué es el Result Pattern?**

**R:** Pattern para **manejo explícito de errores** sin excepciones:

```csharp
// Sin Result Pattern
public void DoSomething()
{
    if (!IsValid())
        throw new Exception("Error"); // ← Oculto
}

// Con Result Pattern
public Result DoSomething()
{
    if (!IsValid())
        return Result.Failure("Error"); // ← Explícito
        
    return Result.Success();
}
```

**Beneficios:**
- Errores **explícitos** en el tipo de retorno
- **Type-safe** - compilador te obliga a manejar
- **Sin excepciones** para flujo de control

### **¿Por qué usar MediatR?**

**R:** MediatR proporciona:
- **Desacoplamiento** - Controller no conoce Handler concreto
- **Pipeline behaviors** - Validación, logging, transacciones
- **CQRS** - Separación clara de Commands y Queries

```csharp
// Sin MediatR
public class TodoListController
{
    private readonly AddTodoItemHandler _addHandler;
    private readonly UpdateTodoItemHandler _updateHandler;
    // ... muchas dependencias
}

// Con MediatR
public class TodoListController
{
    private readonly IMediator _mediator; // ← Una sola dependencia
}
```

### **¿Qué son los Architecture Tests?**

**R:** Tests que **validan reglas arquitectónicas**:

```csharp
[Fact]
public void Domain_Should_NotDependOn_Infrastructure()
{
    var result = Types.InAssembly(domainAssembly)
        .Should()
        .NotHaveDependencyOn("Infrastructure")
        .GetResult();
        
    Assert.True(result.IsSuccessful);
}
```

**Beneficios:**
- **Previenen violaciones** de arquitectura
- **Documentan reglas** de forma ejecutable
- **Fallan en build** si alguien rompe la arquitectura

### **¿Por qué ASP.NET Identity en vez de User aggregate custom?**

**R:** Decision pragmática:
- ✅ **Seguridad probada** - 2FA, lockout, password hashing
- ✅ **Ahorro de tiempo** - no reinventar la rueda
- ✅ **Funcionalidad completa** - roles, claims, tokens
- ⚠️ **Trade-off** - menos control sobre modelo de dominio

Los Value Objects `Username` y `HashedPassword` **demuestran patrones DDD** y podrían usarse en implementación custom futura.

### **¿Qué mejoras tiene pendientes el proyecto?**

**R:** Según el análisis:

1. **Alta prioridad:**
   - Eliminar double `SaveChanges`
   - Remover `GetAllCategories` del repositorio

2. **Media prioridad:**
   - Optimizar handlers (no reconstruir aggregate completo)
   - Actualizar documentación

3. **Baja prioridad:**
   - Usar Specifications consistentemente
   - Remover código no usado

**Rating actual: 8/10** - Arquitectura sólida con oportunidades de refinamiento.

---

## Conceptos Clave para Defender

### 1. **Clean Architecture**
- **Explicar capas** - Domain, Application, Infrastructure, API
- **Regla de dependencia** - Dependencias apuntan hacia adentro
- **Beneficio principal** - Domain independiente de frameworks

### 2. **DDD - Aggregates**
- **TodoList es aggregate root** - único punto de entrada
- **Protege invariantes** - reglas de negocio centralizadas
- **Límite transaccional** - cambios atómicos

### 3. **DDD - Value Objects**
- **Progression es inmutable** - no se puede modificar
- **Auto-validante** - percent entre 0-100 garantizado
- **Igualdad por valor** - dos con mismos valores son iguales

### 4. **SOLID**
- **S** - Cada handler tiene una responsabilidad
- **O** - Extensible via interfaces (ITokenService)
- **L** - ISpecification<T> sustituible
- **I** - Interfaces focalizadas (ICategoryValidator)
- **D** - Dependemos de abstracciones (ITodoListRepository)

### 5. **Patterns**
- **CQRS** - Commands (escribir) separados de Queries (leer)
- **Repository** - Abstracción sobre persistencia
- **Result** - Errores explícitos sin excepciones
- **Specification** - Reglas de negocio encapsuladas

### 6. **Testing**
- **Unit tests** - Domain, Application, Infrastructure
- **Architecture tests** - Validan reglas arquitectónicas
- **Cobertura** - Agregados, handlers, servicios

### 7. **Tecnologías**
- **.NET 8** - Framework moderno LTS
- **MediatR** - CQRS y pipeline behaviors
- **EF Core** - ORM con migraciones
- **JWT** - Autenticación stateless
- **Identity** - Gestión de usuarios probada

---

## Resumen Ejecutivo

Este proyecto es una **implementación profesional** de:

✅ **Clean Architecture** con 4 capas bien separadas
✅ **DDD Táctico** con aggregates, entities, value objects
✅ **SOLID** aplicado consistentemente
✅ **Patrones modernos** (CQRS, Repository, Result, Specification)
✅ **Testing comprehensivo** (unit + architecture tests)

**Rating: 8/10**

**Fortalezas:**
- Arquitectura limpia y escalable
- Lógica de negocio protegida en Domain
- Patterns bien aplicados
- Tests automatizan validación de arquitectura

**Áreas de mejora:**
- Optimización de handlers
- Gestión de transacciones
- Documentación actualizada

**Ideal para:**
- Demostrar conocimientos de arquitectura
- Referencia de Clean Architecture + DDD
- Base para proyectos empresariales

---

**¡Éxito defendiendo tu proyecto! 🚀**

Este proyecto demuestra comprensión sólida de principios arquitectónicos modernos y está bien posicionado como ejemplo de mejores prácticas en .NET.
