using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using TodoListManager.Infrastructure.Persistence;
using TodoListManager.Infrastructure.Persistence.Entities;
using TodoListManager.Infrastructure.Repositories;
using Xunit;

namespace TodoListManager.Infrastructure.Tests.Repositories;

public class GenericRepositoryIntegrationTests
{
    private DbContextOptions<TodoDbContext> CreateInMemoryOptions(string dbName)
    {
        return new DbContextOptionsBuilder<TodoDbContext>()
            .UseInMemoryDatabase(dbName)
            .Options;
    }

    [Fact]
    public async Task Add_Get_Update_Delete_TodoItemEntity_Works()
    {
        var options = CreateInMemoryOptions("testdb1");

        // Add
        using (var context = new TodoDbContext(options))
        {
            var repo = new GenericRepository<TodoItemEntity>(context);
            var item = new TodoItemEntity { Title = "Test", Description = "Desc", Category = "Work" };
            await repo.AddAsync(item);
            item.Id.Should().BeGreaterThan(0);
        }

        // Get
        using (var context = new TodoDbContext(options))
        {
            var repo = new GenericRepository<TodoItemEntity>(context);
            var all = await repo.GetAllAsync();
            all.Should().ContainSingle();
            var first = all.First();
            first.Title.Should().Be("Test");

            // Update
            first.Description = "Updated";
            await repo.UpdateAsync(first);
        }

        // Verify update
        using (var context = new TodoDbContext(options))
        {
            var repo = new GenericRepository<TodoItemEntity>(context);
            var item = (await repo.GetAllAsync()).First();
            item.Description.Should().Be("Updated");

            // Delete
            await repo.DeleteAsync(item);
        }

        // Verify delete
        using (var context = new TodoDbContext(options))
        {
            var repo = new GenericRepository<TodoItemEntity>(context);
            var all = await repo.GetAllAsync();
            all.Should().BeEmpty();
        }
    }
}
