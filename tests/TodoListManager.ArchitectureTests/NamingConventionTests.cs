using NetArchTest.Rules;

namespace TodoListManager.ArchitectureTests;

public class NamingConventionTests
{
    private const string DomainNamespace = "TodoListManager.Domain";
    private const string ApplicationNamespace = "TodoListManager.Application";
    private const string InfrastructureNamespace = "TodoListManager.Infrastructure";

    [Fact]
    public void Interfaces_Should_StartWithI()
    {
        // Arrange
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.PasswordHasher).Assembly;

        // Act & Assert
        var domainResult = Types.InAssembly(domainAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.True(domainResult.IsSuccessful, 
            $"Domain interfaces should start with 'I'. Violations: {string.Join(", ", domainResult.FailingTypeNames ?? [])}");

        var applicationResult = Types.InAssembly(applicationAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.True(applicationResult.IsSuccessful, 
            $"Application interfaces should start with 'I'. Violations: {string.Join(", ", applicationResult.FailingTypeNames ?? [])}");

        var infrastructureResult = Types.InAssembly(infrastructureAssembly)
            .That()
            .AreInterfaces()
            .Should()
            .HaveNameStartingWith("I")
            .GetResult();

        Assert.True(infrastructureResult.IsSuccessful, 
            $"Infrastructure interfaces should start with 'I'. Violations: {string.Join(", ", infrastructureResult.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Exceptions_Should_HaveExceptionSuffix()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .Inherit(typeof(Exception))
            .Should()
            .HaveNameEndingWith("Exception")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Exception classes should end with 'Exception'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Repositories_Should_HaveRepositorySuffix()
    {
        // Arrange
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.PasswordHasher).Assembly;

        // Act
        var result = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Repository")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Repository classes should end with 'Repository'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Services_Should_HaveServiceSuffix()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.PasswordHasher).Assembly;

        // Act & Assert
        var applicationResult = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Services")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Service")
            .GetResult();

        Assert.True(applicationResult.IsSuccessful, 
            $"Application service classes should end with 'Service'. Violations: {string.Join(", ", applicationResult.FailingTypeNames ?? [])}");

        var infrastructureResult = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Services")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Settings")
            .Should()
            .HaveNameEndingWith("Service")
            .Or()
            .HaveNameEndingWith("Validator")
            .Or()
            .HaveNameEndingWith("Hasher")
            .GetResult();

        Assert.True(infrastructureResult.IsSuccessful, 
            $"Infrastructure service classes should end with appropriate suffix. Violations: {string.Join(", ", infrastructureResult.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Validators_Should_HaveValidatorSuffix()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Validators")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Validator")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Validator classes should end with 'Validator'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void CommandHandlers_Should_HaveCommandHandlerSuffix()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Handlers")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("CommandHandler")
            .Should()
            .HaveNameEndingWith("CommandHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Command handler classes should end with 'CommandHandler'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void QueryHandlers_Should_HaveQueryHandlerSuffix()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Handlers")
            .And()
            .AreClasses()
            .And()
            .HaveNameEndingWith("QueryHandler")
            .Should()
            .HaveNameEndingWith("QueryHandler")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Query handler classes should end with 'QueryHandler'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_Should_HaveEventSuffix()
    {
        // Arrange
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(domainAssembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .Should()
            .HaveNameEndingWith("Event")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Domain event classes should end with 'Event'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Specifications_Should_HaveSpecificationSuffix()
    {
        // Arrange
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Only check public concrete specification classes, exclude base classes and internal helpers
        var result = Types.InAssembly(domainAssembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Specifications")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract() // Exclude abstract base class
            .And()
            .ArePublic() // Only check public specifications, exclude internal helpers
            .Should()
            .HaveNameEndingWith("Specification")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Public specification classes should end with 'Specification'. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
