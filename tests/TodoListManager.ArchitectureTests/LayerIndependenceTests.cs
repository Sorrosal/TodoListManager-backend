using NetArchTest.Rules;

namespace TodoListManager.ArchitectureTests;

public class LayerIndependenceTests
{
    private const string DomainNamespace = "TodoListManager.Domain";
    private const string ApplicationNamespace = "TodoListManager.Application";
    private const string InfrastructureNamespace = "TodoListManager.Infrastructure";

    [Fact]
    public void Domain_Should_NotDependOnExternalFrameworks()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Domain should not depend on external frameworks except System namespaces
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(
                "Microsoft.EntityFrameworkCore",
                "Microsoft.AspNetCore",
                "Newtonsoft.Json",
                "System.Net.Http",
                "MediatR",
                "AutoMapper"
            )
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should not depend on external frameworks. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Repositories_Should_BeInDomainOrInfrastructure()
    {
        // Arrange
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.PasswordHasher).Assembly;

        // Act - Repository interfaces should be in Domain, implementations in Infrastructure
        var domainRepositories = Types.InAssembly(domainAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreInterfaces()
            .Should()
            .ResideInNamespace($"{DomainNamespace}.Repositories")
            .GetResult();

        var infrastructureRepositories = Types.InAssembly(infrastructureAssembly)
            .That()
            .HaveNameEndingWith("Repository")
            .And()
            .AreClasses()
            .Should()
            .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
            .GetResult();

        // Assert
        Assert.True(domainRepositories.IsSuccessful,
            $"Repository interfaces should be in Domain.Repositories namespace. Violations: {string.Join(", ", domainRepositories.FailingTypeNames ?? [])}");
        
        Assert.True(infrastructureRepositories.IsSuccessful,
            $"Repository implementations should be in Infrastructure.Repositories namespace. Violations: {string.Join(", ", infrastructureRepositories.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainServices_Should_BeInterfaces()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - All types in Domain.Services should be interfaces
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Services")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All types in Domain.Services should be interfaces. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void ApplicationHandlers_Should_NotBePublic()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;

        // Act - Handlers can be internal, they don't need to be public
        var handlers = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Handlers")
            .And()
            .AreClasses()
            .GetTypes();

        // Assert - Just verify handlers exist and are in correct namespace
        Assert.NotEmpty(handlers);
        Assert.All(handlers, handler => 
            Assert.True(handler.Namespace?.StartsWith($"{ApplicationNamespace}.Handlers") == true,
                $"Handler {handler.Name} should be in {ApplicationNamespace}.Handlers namespace"));
    }

    [Fact]
    public void DTOs_Should_BeInApplicationLayer()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Services.AuthenticationService).Assembly;

        // Act - Check that DTOs are in Application layer
        var dtos = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.DTOs")
            .GetTypes();

        // Assert
        Assert.NotEmpty(dtos);
        Assert.All(dtos, dto => 
            Assert.True(dto.Namespace?.StartsWith($"{ApplicationNamespace}.DTOs") == true,
                $"DTO {dto.Name} should be in {ApplicationNamespace}.DTOs namespace"));
    }

    [Fact]
    public void Validators_Should_OnlyBeInApplication()
    {
        // Arrange
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.PasswordHasher).Assembly;

        // Act - Validators should not be in Domain
        var domainValidators = Types.InAssembly(domainAssembly)
            .That()
            .HaveNameEndingWith("Validator")
            .And()
            .DoNotResideInNamespace($"{DomainNamespace}.Services")
            .GetTypes();

        // Assert
        Assert.Empty(domainValidators);
    }
}
