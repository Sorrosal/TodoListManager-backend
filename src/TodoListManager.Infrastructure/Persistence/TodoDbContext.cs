using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using TodoListManager.Infrastructure.Persistence.Entities;
using TodoListManager.Infrastructure.Identity;
using TodoListManager.Domain.Common;

namespace TodoListManager.Infrastructure.Persistence;

public class TodoDbContext : IdentityDbContext<ApplicationUser, ApplicationRole, int>, IUnitOfWork
{
    public TodoDbContext(DbContextOptions<TodoDbContext> options) : base(options) { }

    public DbSet<TodoItemEntity> TodoItems { get; set; } = null!;
    public DbSet<ProgressionEntity> Progressions { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure TodoItem entity
        modelBuilder.Entity<TodoItemEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Title).IsRequired();
            b.Property(x => x.Description).IsRequired(false);
            b.Property(x => x.Category).IsRequired();
            b.HasMany(x => x.Progressions)
                .WithOne(p => p.TodoItem)
                .HasForeignKey(p => p.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Configure Progression entity
        modelBuilder.Entity<ProgressionEntity>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Date).IsRequired();
            b.Property(x => x.Percent).IsRequired().HasPrecision(5, 2);
        });

        // Configure Identity tables with custom names
        modelBuilder.Entity<ApplicationUser>(b =>
        {
            b.ToTable("Users");
        });

        modelBuilder.Entity<ApplicationRole>(b =>
        {
            b.ToTable("Roles");
        });

        modelBuilder.Entity<IdentityUserRole<int>>(b =>
        {
            b.ToTable("UserRoles");
        });

        modelBuilder.Entity<IdentityUserClaim<int>>(b =>
        {
            b.ToTable("UserClaims");
        });

        modelBuilder.Entity<IdentityUserLogin<int>>(b =>
        {
            b.ToTable("UserLogins");
        });

        modelBuilder.Entity<IdentityUserToken<int>>(b =>
        {
            b.ToTable("UserTokens");
        });

        modelBuilder.Entity<IdentityRoleClaim<int>>(b =>
        {
            b.ToTable("RoleClaims");
        });
    }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        return base.SaveChangesAsync(cancellationToken);
    }
}
