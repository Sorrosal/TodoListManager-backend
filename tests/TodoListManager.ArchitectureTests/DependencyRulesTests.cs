using NetArchTest.Rules;

namespace TodoListManager.ArchitectureTests;

public class DependencyRulesTests
{
    private const string DomainNamespace = "TodoListManager.Domain";
    private const string ApplicationNamespace = "TodoListManager.Application";
    private const string InfrastructureNamespace = "TodoListManager.Infrastructure";
    private const string ApiNamespace = "TodoListManager.API";

    [Fact]
    public void Domain_Should_NotHaveDependencyOnApplication()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApplicationNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Domain layer should not depend on Application layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_NotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Domain layer should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_NotHaveDependencyOnApi()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Domain layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_NotHaveDependencyOnInfrastructure()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Application layer should not depend on Infrastructure layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_NotHaveDependencyOnApi()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Application layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_NotHaveDependencyOnApi()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn(ApiNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful, 
            $"Infrastructure layer should not depend on API layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_DependOnDomain()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Check that Application assembly references Domain assembly
        var referencedAssemblies = applicationAssembly.GetReferencedAssemblies();
        var referencesDomain = referencedAssemblies.Any(a => a.Name == domainAssembly.GetName().Name);

        // Assert
        Assert.True(referencesDomain,
            "Application layer should depend on Domain layer.");
    }

    [Fact]
    public void Infrastructure_Should_DependOnDomain()
    {
        // Arrange
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Check that Infrastructure assembly references Domain assembly
        var referencedAssemblies = infrastructureAssembly.GetReferencedAssemblies();
        var referencesDomain = referencedAssemblies.Any(a => a.Name == domainAssembly.GetName().Name);

        // Assert
        Assert.True(referencesDomain,
            "Infrastructure layer should depend on Domain layer.");
    }
}
