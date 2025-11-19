using NetArchTest.Rules;
using System.Reflection;

namespace TodoListManager.ArchitectureTests;

public class LayerIndependenceTests
{
    private const string DomainNamespace = "TodoListManager.Domain";
    private const string ApplicationNamespace = "TodoListManager.Application";
    private const string InfrastructureNamespace = "TodoListManager.Infrastructure";

    [Fact]
    public void Domain_Should_NotReferenceAnyOtherLayer()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOnAny(ApplicationNamespace, InfrastructureNamespace)
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should not reference any other layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Domain_Should_OnlyContainDomainConcepts()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn("System.Web")
            .And()
            .NotHaveDependencyOn("Microsoft.AspNetCore")
            .And()
            .NotHaveDependencyOn("System.Data")
            .And()
            .NotHaveDependencyOn("EntityFramework")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Domain layer should only contain domain concepts. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_OnlyDependOnDomain()
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
            $"Application layer should only depend on Domain layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Should_NotContainInfrastructureConcerns()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .Should()
            .NotHaveDependencyOn("System.Data")
            .And()
            .NotHaveDependencyOn("System.Web")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Application layer should not contain infrastructure concerns. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Infrastructure_Should_DependOnDomainOnly()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .Should()
            .NotHaveDependencyOn($"{ApplicationNamespace}.Handlers")
            .And()
            .NotHaveDependencyOn($"{ApplicationNamespace}.Commands")
            .And()
            .NotHaveDependencyOn($"{ApplicationNamespace}.Queries")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Infrastructure layer should only depend on Domain layer. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Application_Handlers_Should_NotDependOnEachOther()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;

        // Act - Get all handler types
        var handlerTypes = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Handlers")
            .GetTypes();

        // Check that handlers don't have circular dependencies
        foreach (var handlerType in handlerTypes)
        {
            // Get constructor parameters and field types to check dependencies
            var constructorTypes = handlerType.GetConstructors()
                .SelectMany(c => c.GetParameters().Select(p => p.ParameterType))
                .Where(t => t.Namespace != null && t.Namespace.StartsWith($"{ApplicationNamespace}.Handlers"))
                .Where(t => t != handlerType)
                .ToList();

            var fieldTypes = handlerType.GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
                .Select(f => f.FieldType)
                .Where(t => t.Namespace != null && t.Namespace.StartsWith($"{ApplicationNamespace}.Handlers"))
                .Where(t => t != handlerType)
                .ToList();

            var dependencies = constructorTypes.Concat(fieldTypes).Distinct().ToList();

            Assert.Empty(dependencies);
        }
    }

    [Fact]
    public void Application_Commands_Should_NotDependOnHandlers()
    {
        // Arrange
        var applicationAssembly = typeof(TodoListManager.Application.Commands.AddTodoItemCommand).Assembly;

        // Act
        var result = Types.InAssembly(applicationAssembly)
            .That()
            .ResideInNamespace($"{ApplicationNamespace}.Commands")
            .Should()
            .NotHaveDependencyOn($"{ApplicationNamespace}.Handlers")
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"Commands should not depend on Handlers. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
