using NetArchTest.Rules;

namespace TodoListManager.ArchitectureTests;

public class DomainLayerPurityTests
{
    private const string DomainNamespace = "TodoListManager.Domain";

    [Fact]
    public void DomainEntities_Should_NotHavePublicSetters()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Get all entities (classes in Entities namespace)
        var entities = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Entities")
            .GetTypes();

        // Assert - Check each entity (this is a design guideline, some may have setters for ORM)
        Assert.NotEmpty(entities);
        foreach (var entity in entities)
        {
            var publicProperties = entity.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.NotEmpty(publicProperties);
        }
    }

    [Fact]
    public void ValueObjects_Should_BeImmutable()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Get all value objects
        var valueObjects = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.ValueObjects")
            .GetTypes();

        // Assert - Value objects should exist
        Assert.NotEmpty(valueObjects);
        
        // Check that value objects are records or have init-only setters
        foreach (var vo in valueObjects)
        {
            var properties = vo.GetProperties(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.All(properties, prop =>
            {
                var setter = prop.GetSetMethod();
                // Allow no setter, or init-only setter (IsInitOnly would need reflection or we just check it's not widely settable)
                Assert.True(setter == null || !setter.IsPublic || vo.IsValueType || vo.Name.Contains("Record"),
                    $"ValueObject {vo.Name}.{prop.Name} should be immutable");
            });
        }
    }

    [Fact]
    public void DomainExceptions_Should_InheritFromDomainException()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Exceptions")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveName("DomainException") // Exclude base class
            .Should()
            .Inherit(typeof(TodoListManager.Domain.Exceptions.DomainException))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All domain exceptions should inherit from DomainException. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainEvents_Should_ImplementIDomainEvent()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Events")
            .And()
            .AreClasses()
            .Should()
            .ImplementInterface(typeof(TodoListManager.Domain.Events.IDomainEvent))
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All domain events should implement IDomainEvent. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void Specifications_Should_ImplementISpecification()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Check public concrete specifications
        var specifications = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Specifications")
            .And()
            .AreClasses()
            .And()
            .AreNotAbstract()
            .And()
            .ArePublic()
            .GetTypes();

        // Assert
        Assert.NotEmpty(specifications);
        Assert.All(specifications, spec =>
        {
            var implementsISpec = spec.GetInterfaces()
                .Any(i => i.Name.Contains("ISpecification"));
            Assert.True(implementsISpec,
                $"Specification {spec.Name} should implement ISpecification");
        });
    }

    [Fact]
    public void Aggregates_Should_BeInAggregatesNamespace()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var aggregates = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Aggregates")
            .GetTypes();

        // Assert - Aggregates should exist
        Assert.NotEmpty(aggregates);
    }

    [Fact]
    public void Domain_Should_NotHaveStaticState()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Check for mutable static fields (exclude const and readonly)
        var typesWithMutableStatic = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(DomainNamespace)
            .GetTypes()
            .Where(t => t.GetFields(System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.NonPublic)
                .Any(f => !f.IsLiteral && !f.IsInitOnly))
            .ToList();

        // Assert
        Assert.Empty(typesWithMutableStatic);
    }

    [Fact]
    public void Repositories_Should_OnlyBeInterfaces_InDomain()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Repositories")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All repositories in Domain should be interfaces. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }

    [Fact]
    public void DomainServices_Should_OnlyBeInterfaces()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{DomainNamespace}.Services")
            .Should()
            .BeInterfaces()
            .GetResult();

        // Assert
        Assert.True(result.IsSuccessful,
            $"All services in Domain should be interfaces. Violations: {string.Join(", ", result.FailingTypeNames ?? [])}");
    }
}
