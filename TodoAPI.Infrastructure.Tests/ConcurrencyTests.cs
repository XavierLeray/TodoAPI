using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Exceptions;
using TodoAPI.Infrastructure.Data;
using TodoAPI.Infrastructure.Repositories;
using Xunit;

namespace TodoAPI.Infrastructure.Tests;

// =============================================================================
// TESTS DE CONCURRENCE — Verrouillage optimiste sur TodoItem
// =============================================================================
//
// BUG (Lost Update) — prouvé avec InMemory SANS concurrency token :
//   On crée un DbContext sans le ConcurrencyStamp configuré pour montrer le problème.
//   User A et User B modifient la même ligne → aucune erreur → données perdues.
//
// FIX (Optimistic Locking) — prouvé avec InMemory AVEC concurrency token :
//   Le ConcurrencyStamp est vérifié dans le WHERE du UPDATE.
//   Si modifié entre-temps → DbUpdateConcurrencyException.
//
// Note : avec SQL Server, on pourrait utiliser .IsRowVersion() (type natif "rowversion").
//   Ici on utilise un ConcurrencyStamp (GUID string) + IsConcurrencyToken(),
//   ce qui est portable (InMemory, SQLite, PostgreSQL, SQL Server).
//
// Questions d'entretien :
//   "Comment EF Core détecte le conflit ?"
//   → WHERE Id = @id AND ConcurrencyStamp = @expected → 0 rows → exception.
//
//   "Optimiste vs pessimiste ?"
//   → Optimiste = pas de verrou, détection au save. Bon si conflits rares.
//   → Pessimiste = SELECT FOR UPDATE. Bon si conflits fréquents (stock).
// =============================================================================

public class ConcurrencyTests
{
    /// <summary>
    /// Crée un context InMemory AVEC la config complète (ConcurrencyStamp = concurrency token).
    /// Chaque test reçoit une base isolée grâce au nom unique.
    /// </summary>
    private static DbContextOptions<AppDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_{Guid.NewGuid()}")
            .Options;
    }

    /// <summary>
    /// Crée un context InMemory SANS concurrency token.
    /// Utilisé pour les tests Bug : prouve le lost update quand il n'y a pas de protection.
    /// </summary>
    private static DbContextOptions<NoConcurrencyDbContext> CreateNoConcurrencyOptions()
    {
        return new DbContextOptionsBuilder<NoConcurrencyDbContext>()
            .UseInMemoryDatabase(databaseName: $"TestDb_NoCc_{Guid.NewGuid()}")
            .Options;
    }

    // =========================================================================
    // BUG : Lost Update — sans concurrency token
    // =========================================================================

    [Fact]
    public async Task Bug_LostUpdate_SecondWriteSilentlyOverwritesFirst()
    {
        var options = CreateNoConcurrencyOptions();

        // Seed
        int todoId;
        await using (var setup = new NoConcurrencyDbContext(options))
        {
            var todo = new TodoItem { Title = "Tâche partagée", CreatedAt = DateTime.UtcNow };
            setup.TodoItems.Add(todo);
            await setup.SaveChangesAsync();
            todoId = todo.Id;
        }

        // User A et User B lisent le même todo (deux contexts séparés)
        await using var contextA = new NoConcurrencyDbContext(options);
        await using var contextB = new NoConcurrencyDbContext(options);

        var todoA = await contextA.TodoItems.FindAsync(todoId);
        var todoB = await contextB.TodoItems.FindAsync(todoId);

        // User A modifie et sauvegarde
        todoA!.Title = "Modifié par User A";
        await contextA.SaveChangesAsync();

        // User B modifie et sauvegarde → AUCUNE ERREUR → Lost Update
        todoB!.Title = "Modifié par User B";
        await contextB.SaveChangesAsync();

        // La modification de User A est PERDUE silencieusement
        await using var verify = new NoConcurrencyDbContext(options);
        var finalTodo = await verify.TodoItems.FindAsync(todoId);
        Assert.Equal("Modifié par User B", finalTodo!.Title);
        // User A ne sait pas que son travail a été écrasé.
    }

    [Fact]
    public async Task Bug_LostUpdate_CompletionStatusOverwritten()
    {
        var options = CreateNoConcurrencyOptions();

        int todoId;
        await using (var setup = new NoConcurrencyDbContext(options))
        {
            var todo = new TodoItem { Title = "Valider la commande", IsCompleted = true, CreatedAt = DateTime.UtcNow };
            setup.TodoItems.Add(todo);
            await setup.SaveChangesAsync();
            todoId = todo.Id;
        }

        // User A et User B lisent le même todo
        await using var contextA = new NoConcurrencyDbContext(options);
        await using var contextB = new NoConcurrencyDbContext(options);

        var todoA = await contextA.TodoItems.FindAsync(todoId);
        var todoB = await contextB.TodoItems.FindAsync(todoId);

        // User A change le titre
        todoA!.Title = "Modifié par User A";
        await contextA.SaveChangesAsync();

        // User B remet IsCompleted à false (son intention) et change le titre
        // Sans concurrency token, il écrase les modifications de A sans erreur
        todoB!.IsCompleted = false;
        todoB.Title = "Valider la commande urgente";
        await contextB.SaveChangesAsync();

        await using var verify = new NoConcurrencyDbContext(options);
        var finalTodo = await verify.TodoItems.FindAsync(todoId);

        // BUG confirmé : le titre de User A est perdu, écrasé silencieusement par User B
        Assert.Equal("Valider la commande urgente", finalTodo!.Title);
        Assert.False(finalTodo.IsCompleted,
            "BUG confirmé : User B a écrasé tout sans avertissement");
    }

    // =========================================================================
    // FIX : Verrouillage optimiste — le conflit est détecté
    // =========================================================================

    [Fact]
    public async Task Fix_ConcurrencyConflict_DetectedOnSecondWrite()
    {
        var options = CreateInMemoryOptions();

        int todoId;
        await using (var setup = new AppDbContext(options))
        {
            var todo = TodoItem.Create("Tâche concurrente");
            setup.TodoItems.Add(todo);
            await setup.SaveChangesAsync();
            todoId = todo.Id;
        }

        // User A et User B lisent le même todo
        await using var contextA = new AppDbContext(options);
        await using var contextB = new AppDbContext(options);

        var todoA = await contextA.TodoItems.FirstAsync(t => t.Id == todoId);
        var todoB = await contextB.TodoItems.FirstAsync(t => t.Id == todoId);

        // Même ConcurrencyStamp au départ
        Assert.Equal(todoA.ConcurrencyStamp, todoB.ConcurrencyStamp);

        // User A sauvegarde → OK
        todoA.Title = "Modifié par User A";
        todoA.ConcurrencyStamp = Guid.NewGuid().ToString();
        await contextA.SaveChangesAsync();

        // User B tente avec l'ancien ConcurrencyStamp → CONFLIT
        todoB.Title = "Modifié par User B";
        todoB.ConcurrencyStamp = Guid.NewGuid().ToString();
        await Assert.ThrowsAsync<DbUpdateConcurrencyException>(
            () => contextB.SaveChangesAsync());

        // Seul User A a réussi
        await using var verify = new AppDbContext(options);
        var finalTodo = await verify.TodoItems.FindAsync(todoId);
        Assert.Equal("Modifié par User A", finalTodo!.Title);
    }

    [Fact]
    public async Task Fix_Repository_ThrowsConcurrencyConflictException()
    {
        var options = CreateInMemoryOptions();

        int todoId;
        string originalStamp;
        await using (var setup = new AppDbContext(options))
        {
            var todo = TodoItem.Create("Tâche repo");
            setup.TodoItems.Add(todo);
            await setup.SaveChangesAsync();
            todoId = todo.Id;
            originalStamp = todo.ConcurrencyStamp;
        }

        // User A modifie → ConcurrencyStamp change en base
        await using (var contextA = new AppDbContext(options))
        {
            var repoA = new EfTodoRepository(contextA);
            var updateA = new TodoItem
            {
                Title = "User A",
                IsCompleted = false,
                ConcurrencyStamp = originalStamp
            };
            await repoA.UpdateAsync(todoId, updateA);
        }

        // User B avec l'ANCIEN ConcurrencyStamp → ConcurrencyConflictException
        await using (var contextB = new AppDbContext(options))
        {
            var repoB = new EfTodoRepository(contextB);
            var updateB = new TodoItem
            {
                Title = "User B",
                IsCompleted = false,
                ConcurrencyStamp = originalStamp
            };

            await Assert.ThrowsAsync<ConcurrencyConflictException>(
                () => repoB.UpdateAsync(todoId, updateB));
        }

        // Seul User A a réussi
        await using var verify = new AppDbContext(options);
        var finalTodo = await verify.TodoItems.FindAsync(todoId);
        Assert.Equal("User A", finalTodo!.Title);
    }

    [Fact]
    public async Task Fix_SequentialUpdates_WorkFineWithFreshConcurrencyStamp()
    {
        var options = CreateInMemoryOptions();

        int todoId;
        await using (var setup = new AppDbContext(options))
        {
            var todo = TodoItem.Create("Tâche séquentielle");
            setup.TodoItems.Add(todo);
            await setup.SaveChangesAsync();
            todoId = todo.Id;
        }

        // Update 1 — avec ConcurrencyStamp frais
        await using (var context1 = new AppDbContext(options))
        {
            var repo = new EfTodoRepository(context1);
            var current = await repo.GetByIdAsync(todoId);
            var update = new TodoItem
            {
                Title = "Update 1",
                IsCompleted = false,
                ConcurrencyStamp = current!.ConcurrencyStamp
            };
            var result = await repo.UpdateAsync(todoId, update);
            Assert.NotNull(result);
            Assert.Equal("Update 1", result!.Title);
        }

        // Update 2 — avec ConcurrencyStamp rechargé
        await using (var context2 = new AppDbContext(options))
        {
            var repo = new EfTodoRepository(context2);
            var current = await repo.GetByIdAsync(todoId);
            var update = new TodoItem
            {
                Title = "Update 2",
                IsCompleted = true,
                ConcurrencyStamp = current!.ConcurrencyStamp
            };
            var result = await repo.UpdateAsync(todoId, update);
            Assert.NotNull(result);
            Assert.Equal("Update 2", result!.Title);
        }

        // Vérification finale
        await using var verifyCtx = new AppDbContext(options);
        var finalTodo = await verifyCtx.TodoItems.FindAsync(todoId);
        Assert.Equal("Update 2", finalTodo!.Title);
        Assert.True(finalTodo.IsCompleted);
    }
}

// =============================================================================
// DbContext SANS concurrency token — utilisé uniquement pour les tests Bug
// =============================================================================
// Ce context est volontairement "nu" : il prouve que sans protection,
// le lost update se produit silencieusement.
// =============================================================================

public class NoConcurrencyDbContext : DbContext
{
    public NoConcurrencyDbContext(DbContextOptions<NoConcurrencyDbContext> options)
        : base(options) { }

    public DbSet<TodoItem> TodoItems => Set<TodoItem>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<TodoItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Title).IsRequired().HasMaxLength(200);
            entity.Property(e => e.IsCompleted).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();
            // PAS de IsConcurrencyToken → pas de protection → lost update
            entity.Ignore(e => e.ConcurrencyStamp);
            entity.Ignore(e => e.Category);
            entity.Ignore(e => e.CategoryId);
            entity.Ignore(e => e.TodoItemTags);
        });
    }
}
