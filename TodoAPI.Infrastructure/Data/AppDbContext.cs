using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Constants;
using TodoAPI.Domain.Entities;

namespace TodoAPI.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();
    public DbSet<Category> Categories => Set<Category>();
    public DbSet<Tag> Tags => Set<Tag>();
    public DbSet<TodoItemTag> TodoItemTags => Set<TodoItemTag>();
    public DbSet<User> Users => Set<User>();
    public DbSet<Role> RolesTable => Set<Role>();
    public DbSet<UserRole> UserRoles => Set<UserRole>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsCompleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasOne(t => t.Category)
                .WithMany(c => c.TodoItems)
                .HasForeignKey(t => t.CategoryId)
                .OnDelete(DeleteBehavior.SetNull);

            // Concurrency token — verrouillage optimiste
            // Le ConcurrencyStamp est un GUID régénéré par le repository à chaque UPDATE.
            // EF Core l'inclut dans la clause WHERE :
            //   UPDATE TodoItems SET ... WHERE Id = @id AND ConcurrencyStamp = @expected
            // Si un autre utilisateur a modifié la ligne → 0 rows → DbUpdateConcurrencyException.
            //
            // Avantage du GUID string vs byte[] rowversion :
            //   - Portable (SQLite, InMemory, PostgreSQL, SQL Server)
            //   - Pas de HasDefaultValueSql / ValueGeneratedOnAddOrUpdate
            //   - Sérialisation JSON directe (pas de Base64)
            entity.Property(e => e.ConcurrencyStamp)
                .IsConcurrencyToken()
                .HasMaxLength(36);
        });

        modelBuilder.Entity<Category>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(7);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new Category { Id = 1, Name = "Personnel", Color = "#3498db", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 2, Name = "Travail", Color = "#e74c3c", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Category { Id = 3, Name = "Courses", Color = "#2ecc71", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<Tag>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(30);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new Tag { Id = 1, Name = "urgent", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tag { Id = 2, Name = "important", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Tag { Id = 3, Name = "facile", CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<TodoItemTag>(entity =>
        {
            entity.HasKey(tt => new { tt.TodoItemId, tt.TagId });

            entity.HasOne(tt => tt.TodoItem)
                .WithMany(t => t.TodoItemTags)
                .HasForeignKey(tt => tt.TodoItemId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(tt => tt.Tag)
                .WithMany(tag => tag.TodoItemTags)
                .HasForeignKey(tt => tt.TagId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AssignedAt).IsRequired();
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(100);
            entity.Property(e => e.PasswordHash).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.Email).IsUnique();

            entity.HasData(new User
            {
                Id = 1,
                Username = "admin",
                Email = "admin@todoapi.com",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
                CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });

        modelBuilder.Entity<Role>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(50);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.HasIndex(e => e.Name).IsUnique();

            entity.HasData(
                new Role { Id = 1, Name = Roles.Admin, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) },
                new Role { Id = 2, Name = Roles.User, CreatedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc) }
            );
        });

        modelBuilder.Entity<UserRole>(entity =>
        {
            entity.HasKey(ur => new { ur.UserId, ur.RoleId });

            entity.HasOne(ur => ur.User)
                .WithMany(u => u.UserRoles)
                .HasForeignKey(ur => ur.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ur => ur.Role)
                .WithMany(r => r.UserRoles)
                .HasForeignKey(ur => ur.RoleId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.Property(e => e.AssignedAt).IsRequired();

            entity.HasData(new UserRole
            {
                UserId = 1,
                RoleId = 1,
                AssignedAt = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc)
            });
        });
    }
}