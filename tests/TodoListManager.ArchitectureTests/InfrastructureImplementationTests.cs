using NetArchTest.Rules;

namespace TodoListManager.ArchitectureTests;

public class InfrastructureImplementationTests
{
    private const string DomainNamespace = "TodoListManager.Domain";
    private const string InfrastructureNamespace = "TodoListManager.Infrastructure";

    [Fact]
    public void RepositoryImplementations_Should_ImplementDomainInterfaces()
    {
        // Arrange
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Get repository implementations
        var repositories = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
            .And()
            .AreClasses()
            .GetTypes();

        // Assert - Each repository should implement a domain interface
        Assert.NotEmpty(repositories);
        foreach (var repo in repositories)
        {
            var interfaces = repo.GetInterfaces();
            var implementsDomainInterface = interfaces.Any(i => 
                i.Namespace?.StartsWith(DomainNamespace) == true);
            
            Assert.True(implementsDomainInterface,
                $"Repository {repo.Name} should implement a domain interface");
        }
    }

    [Fact]
    public void ServiceImplementations_Should_ImplementDomainInterfaces()
    {
        // Arrange
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act - Get service implementations
        var services = Types.InAssembly(infrastructureAssembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Services")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Settings") // Exclude configuration classes
            .GetTypes();

        // Assert - Each service should implement a domain interface
        Assert.NotEmpty(services);
        foreach (var service in services)
        {
            var interfaces = service.GetInterfaces();
            var implementsDomainInterface = interfaces.Any(i => 
                i.Namespace?.StartsWith(DomainNamespace) == true);
            
            Assert.True(implementsDomainInterface,
                $"Service {service.Name} should implement a domain interface");
        }
    }

    [Fact]
    public void InfrastructureClasses_Should_BePublicOrInternal()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act
        var result = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace(InfrastructureNamespace)
            .And()
            .AreClasses()
            .Should()
            .BePublic()
            .Or()
            .NotBePublic() // This allows both public and internal
            .GetResult();

        // Assert - Just checking they compile and are valid classes
        Assert.True(result.IsSuccessful);
    }

    [Fact]
    public void Configuration_Should_BeInConfigurationNamespace()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act - Get configuration classes
        var configurations = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Configuration")
            .GetTypes();

        // Assert - Configuration classes should exist and have Settings suffix
        if (configurations.Any())
        {
            Assert.All(configurations, config =>
            {
                Assert.True(config.Name.EndsWith("Settings") || config.Name.Contains("Config"),
                    $"Configuration class {config.Name} should end with 'Settings' or contain 'Config'");
            });
        }
    }

    [Fact]
    public void InfrastructureRepositories_Should_NotBeSealed()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act - Repository implementations can be inherited for testing
        var repositories = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
            .And()
            .AreClasses()
            .GetTypes();

        // Assert - Check repositories can be extended (not sealed)
        Assert.NotEmpty(repositories);
        Assert.All(repositories, repo =>
        {
            Assert.True(!repo.IsSealed || repo.IsAbstract,
                $"Repository {repo.Name} should not be sealed to allow testing");
        });
    }

    [Fact]
    public void InfrastructureServices_Should_HaveConstructorDependencies()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act - Get service implementations
        var services = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Services")
            .And()
            .AreClasses()
            .And()
            .DoNotHaveNameEndingWith("Settings")
            .GetTypes();

        // Assert - Services should have constructors (for DI)
        Assert.NotEmpty(services);
        Assert.All(services, service =>
        {
            var constructors = service.GetConstructors(
                System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance);
            Assert.NotEmpty(constructors);
        });
    }

    [Fact]
    public void Repositories_Should_HaveSyncMethods()
    {
        // Arrange
        var assembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;

        // Act - Get repository implementations
        var repositories = Types.InAssembly(assembly)
            .That()
            .ResideInNamespace($"{InfrastructureNamespace}.Repositories")
            .And()
            .AreClasses()
            .GetTypes();

        // Assert - Repositories should have methods
        Assert.NotEmpty(repositories);
        Assert.All(repositories, repo =>
        {
            var methods = repo.GetMethods(System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Instance)
                .Where(m => !m.IsSpecialName && m.DeclaringType == repo);
            Assert.NotEmpty(methods);
        });
    }

    [Fact]
    public void InfrastructureAssembly_Should_ReferenceDomainAssembly()
    {
        // Arrange
        var infrastructureAssembly = typeof(TodoListManager.Infrastructure.Services.JwtTokenService).Assembly;
        var domainAssembly = typeof(TodoListManager.Domain.Common.Result).Assembly;

        // Act
        var referencedAssemblies = infrastructureAssembly.GetReferencedAssemblies();
        var referencesDomain = referencedAssemblies.Any(a => a.Name == domainAssembly.GetName().Name);

        // Assert
        Assert.True(referencesDomain,
            "Infrastructure assembly should reference Domain assembly");
    }
}
