# TodoListManager Backend API

A professionally architected .NET 8 Web API for managing todo lists with progression tracking, built using **Clean Architecture**, **Domain-Driven Design (DDD)**, and **SOLID principles**.

## ⚡ Quick Start

### 🔑 Default Admin Credentials

The application comes pre-configured with an admin user:

| Field    | Value   |
|----------|---------|
| **Username** | `admin` |
| **Password** | `admin` |

Use these credentials to login via `/api/v1/Auth/login` and get your JWT token.

### 🗄️ Database Configuration

**IMPORTANT**: Before running the application, configure your database connection string.

Edit the file `src/TodoListManager.API/appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SERVER;Database=TodoListDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
  }
}
```

**Connection String Examples:**

**SQL Server Express (Local):**
```json
"DefaultConnection": "Server=localhost\\SQLEXPRESS;Database=TodoListDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**SQL Server with Username/Password:**
```json
"DefaultConnection": "Server=localhost;Database=TodoListDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**SQL Server (Named Instance):**
```json
"DefaultConnection": "Server=localhost\\INSTANCE_NAME;Database=TodoListDb;Trusted_Connection=True;TrustServerCertificate=True;MultipleActiveResultSets=true"
```

**Azure SQL Database:**
```json
"DefaultConnection": "Server=tcp:yourserver.database.windows.net,1433;Database=TodoListDb;User Id=YOUR_USER;Password=YOUR_PASSWORD;Encrypt=True;TrustServerCertificate=False;Connection Timeout=30;"
```

> 💡 **Tip**: The database will be created automatically on first run if it doesn't exist.

---

## 📋 Table of Contents

- [Quick Start](#-quick-start)
  - [Default Admin Credentials](#-default-admin-credentials)
  - [Database Configuration](#️-database-configuration)
- [Features](#-features)
- [Architecture](#-architecture)
- [Testing](#-testing)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the API](#running-the-api)
  - [Running Tests](#running-tests)
- [API Documentation](#-api-documentation)
- [Project Structure](#-project-structure)
- [Design Patterns & Principles](#-design-patterns--principles)
- [Technologies](#-technologies)
- [License](#-license)

## 🚀 Features

- ✅ **CRUD Operations** for Todo Items with category management
- 📊 **Progression Tracking** with percentage-based progress
- 🔐 **JWT Authentication** with role-based authorization
- 🛡️ **Business Rules Enforcement** (e.g., items with >50% progress cannot be modified)
- ✔️ **Validation Pipeline** using FluentValidation
- 📖 **API Versioning** with Swagger/OpenAPI documentation
- 🎯 **CQRS Pattern** implementation with MediatR
- 🏗️ **Clean Architecture** with clear separation of concerns
- 🔄 **Domain-Driven Design** with aggregates, entities, and value objects
- 🎁 **Result Pattern** for explicit error handling and type-safe responses
- 🧪 **Comprehensive Test Coverage** with unit tests and architecture tests

## 🏛️ Architecture

This project follows **Clean Architecture** principles with a clear separation of concerns across four layers:

```
┌─────────────────────────────────────────────────────┐
│                    API Layer                        │
│  (Controllers, Middleware, Configuration)           │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│               Application Layer                     │
│  (Use Cases, Commands, Queries, Handlers)           │
└─────────────────────────────────────────────────────┘
                        ↓
┌─────────────────────────────────────────────────────┐
│                 Domain Layer                        │
│  (Entities, Aggregates, Value Objects, Rules)       │
└─────────────────────────────────────────────────────┘
                        ↑
┌─────────────────────────────────────────────────────┐
│             Infrastructure Layer                    │
│  (Data Access, External Services, JWT)              │
└─────────────────────────────────────────────────────┘
```

### 📚 Detailed Documentation

- **[Clean Architecture](docs/CLEAN_ARCHITECTURE.md)** - Layer separation, dependency rules, and implementation details
- **[Domain-Driven Design](docs/DDD.md)** - Aggregates, entities, value objects, and ubiquitous language
- **[SOLID Principles](docs/SOLID.md)** - How each SOLID principle is applied with code examples
- **[Setup Guide](docs/SETUP.md)** - Detailed installation and configuration instructions

## 🧪 Testing

This project implements a comprehensive testing strategy to ensure code quality, maintainability, and architectural integrity.

### Test Projects

The solution includes multiple test projects covering different aspects:

#### 🔬 Unit Tests
- **TodoListManager.Domain.Tests** - Domain layer unit tests
  - Aggregate behavior validation
  - Entity and value object tests
  - Business rule enforcement
  - Specification pattern tests

- **TodoListManager.Application.Tests** - Application layer unit tests
  - Command and query handler tests
  - Validation logic tests
  - Use case scenarios
  - Service interaction tests

- **TodoListManager.Infrastructure.Tests** - Infrastructure layer unit tests
  - Repository implementation tests
  - Service implementation tests
  - JWT token generation and validation
  - Password hashing verification

#### 🏛️ Architecture Tests
- **TodoListManager.ArchitectureTests** - Architectural rule enforcement
  - Layer dependency validation (ensures dependency rules are not violated)
  - Clean Architecture boundaries enforcement
  - Naming convention validation
  - Project reference verification
  - Ensures Domain layer has no external dependencies

### Running Tests

Run all tests:
```bash
dotnet test
```

Run specific test project:
```bash
dotnet test tests/TodoListManager.Domain.Tests
dotnet test tests/TodoListManager.Application.Tests
dotnet test tests/TodoListManager.Infrastructure.Tests
dotnet test tests/TodoListManager.ArchitectureTests
```

Run tests with coverage:
```bash
dotnet test --collect:"XPlat Code Coverage"
```

### Test Coverage
The test suite covers:
- ✅ Domain entities and aggregates
- ✅ Value objects and specifications
- ✅ Command and query handlers
- ✅ Validation behaviors
- ✅ Repository implementations
- ✅ Authentication and authorization
- ✅ Architectural boundaries

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- SQL Server (Express, Developer, or Azure SQL Database)
- Your favorite IDE ([Visual Studio 2022](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/))
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/Sorrosal/TodoListManager-backend.git
   cd TodoListManager-backend
   ```

2. **Configure the database connection string**
   
   Edit `src/TodoListManager.API/appsettings.Development.json` and update the connection string to match your SQL Server instance (see [Database Configuration](#️-database-configuration) section above).

3. **Restore dependencies**
   ```bash
   dotnet restore
   ```

4. **Build the solution**
   ```bash
   dotnet build
   ```

### Running the API

1. **Navigate to the API project**
   ```bash
   cd src/TodoListManager.API
   ```

2. **Run the application**
   ```bash
   dotnet run
   ```

3. **Access the API**
   - API Base URL: `https://localhost:7xxx` (port will be shown in console)
   - Swagger UI: `https://localhost:7xxx/swagger`

4. **Login with default credentials**
   - Use the admin credentials from the [Quick Start](#-quick-start) section
   - Navigate to `/api/v1/Auth/login` in Swagger
   - Get your JWT token and authorize

### Running Tests

From the solution root directory:

```bash
# Run all tests
dotnet test

# Run with detailed output
dotnet test --logger "console;verbosity=detailed"

# Run only architecture tests
dotnet test tests/TodoListManager.ArchitectureTests

# Run with code coverage
dotnet test --collect:"XPlat Code Coverage"
```

## 📖 API Documentation

### Available Endpoints

#### Authentication
- `POST /api/v1/Auth/login` - Authenticate and get JWT token
- `GET /api/v1/Auth/me` - Get current user information

#### Todo List Management
- `GET /api/v1/TodoList` - Get all todo items
- `POST /api/v1/TodoList` - Create a new todo item
- `PUT /api/v1/TodoList/{id}` - Update a todo item description
- `DELETE /api/v1/TodoList/{id}` - Remove a todo item
- `POST /api/v1/TodoList/{id}/progression` - Register progress for a todo item

### Example Request: Login

```bash
curl -X POST "https://localhost:7xxx/api/v1/Auth/login" \
  -H "Content-Type: application/json" \
  -d '{
    "username": "admin",
    "password": "admin"
  }'
```

**Response:**
```json
{
  "token": "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9...",
  "expiration": "2024-01-15T11:00:00Z"
}
```

### Example Request: Create Todo Item

```bash
curl -X POST "https://localhost:7xxx/api/v1/TodoList" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "title": "Learn Clean Architecture",
    "description": "Study and implement Clean Architecture principles",
    "category": "Education"
  }'
```

### Example Request: Register Progression

```bash
curl -X POST "https://localhost:7xxx/api/v1/TodoList/1/progression" \
  -H "Authorization: Bearer YOUR_TOKEN" \
  -H "Content-Type: application/json" \
  -d '{
    "id": 1,
    "date": "2024-01-15T10:00:00Z",
    "percent": 25.5
  }'
```

> 📖 Full API documentation is available via Swagger UI when running the application

## 📁 Project Structure

```
TodoListManager-backend/
│
├── src/
│   ├── TodoListManager.Domain/           # Domain Layer
│   │   ├── Aggregates/                   # Aggregate Roots (TodoList, User)
│   │   ├── Entities/                     # Domain Entities (TodoItem, Role, Permission)
│   │   ├── ValueObjects/                 # Value Objects (Progression, Username)
│   │   ├── Specifications/               # Business Rule Specifications
│   │   ├── Services/                     # Domain Services Interfaces
│   │   ├── Repositories/                 # Repository Interfaces
│   │   ├── Exceptions/                   # Domain-specific Exceptions
│   │   └── Events/                       # Domain Events
│   │
│   ├── TodoListManager.Application/      # Application Layer
│   │   ├── Commands/                     # Command definitions (CQRS)
│   │   ├── Queries/                      # Query definitions (CQRS)
│   │   ├── Handlers/                     # Command/Query Handlers
│   │   ├── Validators/                   # FluentValidation validators
│   │   ├── Behaviors/                    # MediatR Pipeline Behaviors
│   │   ├── Services/                     # Application Services
│   │   └── DTOs/                         # Data Transfer Objects
│   │
│   ├── TodoListManager.Infrastructure/   # Infrastructure Layer
│   │   ├── Repositories/                 # Repository Implementations
│   │   ├── Services/                     # Service Implementations (JWT, Hashing)
│   │   └── Configuration/                # Infrastructure Configuration
│   │
│   └── TodoListManager.API/              # Presentation Layer
│       ├── Controllers/                  # API Controllers
│       ├── Extensions/                   # Service Registration Extensions
│       ├── Configuration/                # API Configuration (Swagger)
│       └── Program.cs                    # Application Entry Point
│
├── tests/                                # Test Projects
│   ├── TodoListManager.Domain.Tests/     # Domain layer unit tests
│   ├── TodoListManager.Application.Tests/# Application layer unit tests
│   ├── TodoListManager.Infrastructure.Tests/ # Infrastructure tests
│   └── TodoListManager.ArchitectureTests/# Architecture rules tests
│
├── docs/                                 # Documentation
│   ├── CLEAN_ARCHITECTURE.md             # Clean Architecture details
│   ├── DDD.md                            # Domain-Driven Design details
│   ├── SOLID.md                          # SOLID principles examples
│   └── SETUP.md                          # Detailed setup guide
│
└── README.md                             # This file
```

## 🎯 Design Patterns & Principles

This project demonstrates professional software engineering practices:

### [SOLID Principles](docs/SOLID.md)
- **S**ingle Responsibility Principle - Each class has one reason to change
- **O**pen/Closed Principle - Open for extension, closed for modification
- **L**iskov Substitution Principle - Derived classes can substitute base classes
- **I**nterface Segregation Principle - Many specific interfaces over one general interface
- **D**ependency Inversion Principle - Depend on abstractions, not concretions

### [Domain-Driven Design](docs/DDD.md)
- **Aggregates** - TodoList and User aggregates with consistency boundaries
- **Entities** - TodoItem, Role, Permission with identity
- **Value Objects** - Progression, Username, HashedPassword
- **Domain Services** - ICategoryValidator, IPasswordHasher
- **Specifications** - Business rule encapsulation
- **Domain Events** - Cross-aggregate communication

### [Clean Architecture](docs/CLEAN_ARCHITECTURE.md)
- **Dependency Rule** - Dependencies point inward
- **Layer Separation** - Domain → Application → Infrastructure → API
- **Use Case Driven** - Business logic in application layer
- **Framework Independence** - Core business logic independent of frameworks

### Design Patterns
- **CQRS** (Command Query Responsibility Segregation) - Separate read and write operations
- **Repository Pattern** - Abstraction over data access
- **Specification Pattern** - Encapsulate business rules
- **Result Pattern** - Explicit error handling without exceptions
- **Factory Pattern** - Object creation (User.Create)
- **Pipeline Pattern** - MediatR behaviors (ValidationBehavior)
- **Strategy Pattern** - Authentication strategies

### Result Pattern Implementation

This project implements the **Result Pattern** for robust error handling:

#### Benefits
- **Type-Safe Error Handling** - Compile-time guarantees for error cases
- **Explicit Success/Failure** - No hidden exceptions or null returns
- **Rich Error Information** - Detailed error messages with context
- **Railway-Oriented Programming** - Chain operations with automatic short-circuiting
- **Improved Testability** - Easy to test success and failure paths

#### Usage Examples

**Command Handlers:**
```csharp
public async Task<Result<TodoItemDto>> Handle(CreateTodoItemCommand request, CancellationToken cancellationToken)
{
    // Validation failures return Result.Failure
    if (validation.IsInvalid)
        return Result.Failure<TodoItemDto>("Invalid input");
    
    // Business rule violations return Result.Failure
    if (!businessRule.IsSatisfied)
        return Result.Failure<TodoItemDto>("Business rule violated");
    
    // Success returns Result.Success with data
    return Result.Success(todoItemDto);
}
```

**API Controllers:**
```csharp
var result = await _mediator.Send(command);

if (result.IsFailure)
    return BadRequest(result.Error);

return Ok(result.Value);
```

**Benefits in Practice:**
- No try-catch blocks cluttering business logic
- Clear separation between expected failures and exceptional cases
- Self-documenting code (return type shows operation can fail)
- Easier to compose operations and maintain control flow

## 🛠️ Technologies

### Core Framework
- **.NET 8** - Latest long-term support version
- **C# 12** - Latest language features

### Libraries & Packages
- **ASP.NET Core 8** - Web API framework
- **Entity Framework Core 8** - ORM for data access
- **MediatR** - CQRS and mediator pattern implementation
- **FluentValidation** - Fluent validation library
- **JWT Bearer Authentication** - Token-based authentication
- **Swashbuckle (Swagger)** - API documentation
- **Asp.Versioning** - API versioning support

### Testing Libraries
- **xUnit** - Unit testing framework
- **FluentAssertions** - Fluent assertion library for tests
- **Moq** - Mocking framework for unit tests
- **NetArchTest.Rules** - Architecture testing library

### Patterns & Practices
- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID Principles
- CQRS Pattern
- Repository Pattern
- Specification Pattern
- Result Pattern

## 📝 Key Business Rules

The domain layer enforces critical business rules:

1. **Progression Constraints**
   - Percent must be between 0 and 100
   - Total progress cannot exceed 100%
   - Progression dates must be chronological

2. **Modification Rules**
   - Todo items with >50% progress cannot be modified or deleted
   - Category must be valid (validated by domain service)

3. **User Management**
   - Users must have at least one role
   - Usernames must be unique
   - Passwords are hashed using secure algorithms

## 🤝 Contributing

This is an educational project demonstrating architectural patterns. Feel free to fork and explore!

## 📄 License

Copyright (c) Sergio Sorrosal. All Rights Reserved.

---

## 📚 Learning Resources

To understand the architecture and design decisions in this project:

1. Start with **[Clean Architecture](docs/CLEAN_ARCHITECTURE.md)** to understand the layer structure
2. Read **[Domain-Driven Design](docs/DDD.md)** to understand the domain model
3. Review **[SOLID Principles](docs/SOLID.md)** to see how principles are applied
4. Follow the **[Setup Guide](docs/SETUP.md)** to run and experiment with the API

---

**Built with ❤️ using Clean Architecture and Domain-Driven Design principles**
