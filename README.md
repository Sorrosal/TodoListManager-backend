# TodoListManager Backend API

[![.NET 8](https://img.shields.io/badge/.NET-8.0-purple)](https://dotnet.microsoft.com/)
[![C# 12](https://img.shields.io/badge/C%23-12.0-blue)](https://docs.microsoft.com/en-us/dotnet/csharp/)
[![Clean Architecture](https://img.shields.io/badge/Architecture-Clean-green)](docs/CLEAN_ARCHITECTURE.md)
[![DDD](https://img.shields.io/badge/Design-DDD-orange)](docs/DDD.md)
[![SOLID](https://img.shields.io/badge/Principles-SOLID-red)](docs/SOLID.md)

A professionally architected .NET 8 Web API for managing todo lists with progression tracking, built using **Clean Architecture**, **Domain-Driven Design (DDD)**, and **SOLID principles**.

## 📋 Table of Contents

- [Features](#-features)
- [Architecture](#-architecture)
- [Getting Started](#-getting-started)
  - [Prerequisites](#prerequisites)
  - [Installation](#installation)
  - [Running the API](#running-the-api)
  - [Default Credentials](#default-credentials)
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

## 🚀 Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) or later
- Your favorite IDE ([Visual Studio 2022](https://visualstudio.microsoft.com/), [Rider](https://www.jetbrains.com/rider/), or [VS Code](https://code.visualstudio.com/))
- [Git](https://git-scm.com/)

### Installation

1. **Clone the repository**
   ```bash
   git clone https://github.com/sergio-sorrosal_inetcat/TodoListManager-backend.git
   cd TodoListManager-backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Build the solution**
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

### Default Credentials

The application is pre-configured with an **admin user** for testing:

| Field    | Value   |
|----------|---------|
| Username | `admin` |
| Password | `admin` |

**Authentication Flow:**
1. Navigate to Swagger UI: `https://localhost:7xxx/swagger`
2. Use the `/api/v1/Auth/login` endpoint
3. POST request body:
   ```json
   {
     "username": "admin",
     "password": "admin"
   }
   ```
4. Copy the JWT token from the response
5. Click **"Authorize"** button in Swagger
6. Enter: `Bearer YOUR_TOKEN_HERE`
7. Now you can access protected endpoints!

**Admin Role Permissions:**
- ✅ TodoListRead
- ✅ TodoListCreate
- ✅ TodoListUpdate
- ✅ TodoListDelete
- ✅ TodoListManage

> 📖 For more detailed setup instructions, see [SETUP.md](docs/SETUP.md)

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
- **Factory Pattern** - Object creation (User.Create)
- **Pipeline Pattern** - MediatR behaviors (ValidationBehavior)
- **Strategy Pattern** - Authentication strategies

## 🛠️ Technologies

### Core Framework
- **.NET 8** - Latest long-term support version
- **C# 12** - Latest language features

### Libraries & Packages
- **ASP.NET Core 8** - Web API framework
- **MediatR** - CQRS and mediator pattern implementation
- **FluentValidation** - Fluent validation library
- **JWT Bearer Authentication** - Token-based authentication
- **Swashbuckle (Swagger)** - API documentation
- **Asp.Versioning** - API versioning support

### Patterns & Practices
- Clean Architecture
- Domain-Driven Design (DDD)
- SOLID Principles
- CQRS Pattern
- Repository Pattern
- Specification Pattern

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
