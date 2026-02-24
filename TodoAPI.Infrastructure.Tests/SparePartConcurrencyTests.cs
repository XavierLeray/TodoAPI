using Microsoft.EntityFrameworkCore;
using TodoAPI.Domain.Entities;
using TodoAPI.Domain.Exceptions;
using TodoAPI.Infrastructure.Data;
using TodoAPI.Infrastructure.Repositories;
using Xunit;

namespace TodoAPI.Infrastructure.Tests;

// =============================================================================
// TESTS DE CONCURRENCE — Pièces auto d'occasion (module Parts)
// =============================================================================
//
// Scénario métier : plusieurs acheteurs depuis différents canaux
// (opisto.fr, opisto.pro, oscaro) tentent de réserver la même pièce unique.
//
// BUG (Overselling) — prouvé SANS vérification du ConcurrencyStamp :
//   On modifie directement le context sans passer par le repository.
//   Plusieurs réservations passent → stock négatif → survente.
//
// FIX (Optimistic Locking) — prouvé AVEC le repository qui vérifie le stamp :
//   Le ConcurrencyStamp est vérifié manuellement dans ReserveAsync.
//   Seule la première réservation réussit, les autres lèvent ConcurrencyConflictException.
// =============================================================================

public class SparePartConcurrencyTests
{
    private static DbContextOptions<AppDbContext> CreateInMemoryOptions()
    {
        return new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: $"SparePartTestDb_{Guid.NewGuid()}")
            .Options;
    }

    private static SparePart CreateTestPart(int id = 1)
    {
        return new SparePart
        {
            Id = id,
            Reference = "TEST-001",
            Description = "Pièce test",
            VhuCenter = "VHU Test #1",
            Price = 50.00m,
            StockQuantity = 1,
            Status = PartStatus.Available,
            CreatedAt = DateTime.UtcNow,
            ConcurrencyStamp = Guid.NewGuid().ToString()
        };
    }

    // =========================================================================
    // BUG : Overselling — sans vérification du ConcurrencyStamp
    // =========================================================================

    [Fact]
    public async Task Bug_Overselling_MultipleBuyersGetSamePart()
    {
        // Arrange : une pièce unique (stock = 1)
        var options = CreateInMemoryOptions();
        string partStamp;

        await using (var setup = new AppDbContext(options))
        {
            var part = CreateTestPart();
            partStamp = part.ConcurrencyStamp;
            setup.SpareParts.Add(part);
            await setup.SaveChangesAsync();
        }

        // Act : 5 réservations séquentielles SANS vérifier le stamp
        // (on utilise directement le context, pas le repo)
        var successCount = 0;
        var buyers = new[] { "opisto.fr:Alice", "opisto.pro:Bob", "oscaro:Charlie", "opisto.fr:David", "opisto.pro:Eve" };

        foreach (var buyer in buyers)
        {
            await using var context = new AppDbContext(options);
            var part = await context.SpareParts.FindAsync(1);

            // PAS de vérification du stamp ni du status → toutes passent
            part!.StockQuantity--;
            part.ReservedByBuyer = buyer;
            part.Status = PartStatus.Reserved;
            // On force le nouveau stamp pour que EF Core ne bloque pas
            part.ConcurrencyStamp = Guid.NewGuid().ToString();
            await context.SaveChangesAsync();
            successCount++;
        }

        // Assert : les 5 ont réussi → BUG, la pièce a été "vendue" 5 fois
        Assert.Equal(5, successCount);

        await using var verify = new AppDbContext(options);
        var finalPart = await verify.SpareParts.FindAsync(1);
        Assert.True(finalPart!.StockQuantity < 0,
            $"BUG confirmé : stock = {finalPart.StockQuantity}, la pièce a été survendue");
    }

    [Fact]
    public async Task Bug_Overselling_StockGoesNegative()
    {
        // Arrange : une pièce unique (stock = 1)
        var options = CreateInMemoryOptions();

        await using (var setup = new AppDbContext(options))
        {
            setup.SpareParts.Add(CreateTestPart());
            await setup.SaveChangesAsync();
        }

        // Act : 5 réservations séquentielles SANS vérifier le stamp
        for (int i = 0; i < 5; i++)
        {
            await using var context = new AppDbContext(options);
            var part = await context.SpareParts.FindAsync(1);
            part!.StockQuantity--;
            part.ConcurrencyStamp = Guid.NewGuid().ToString();
            await context.SaveChangesAsync();
        }

        // Assert : stock négatif → survente
        await using var verify = new AppDbContext(options);
        var finalPart = await verify.SpareParts.FindAsync(1);
        Assert.True(finalPart!.StockQuantity < 0,
            $"BUG confirmé : stock = {finalPart.StockQuantity} (devrait être >= 0)");
    }

    // =========================================================================
    // FIX : Verrouillage optimiste — ConcurrencyStamp vérifié par le repository
    // =========================================================================

    [Fact]
    public async Task Fix_OnlyOneBuyerReserves()
    {
        // Arrange : une pièce unique (stock = 1)
        var options = CreateInMemoryOptions();
        string initialStamp;

        await using (var setup = new AppDbContext(options))
        {
            var part = CreateTestPart();
            initialStamp = part.ConcurrencyStamp;
            setup.SpareParts.Add(part);
            await setup.SaveChangesAsync();
        }

        // Act : 5 tâches concurrentes avec le MÊME ConcurrencyStamp initial
        var channels = new[] { "opisto.fr", "opisto.pro", "oscaro", "opisto.fr", "opisto.pro" };
        var buyers = new[] { "Alice", "Bob", "Charlie", "David", "Eve" };
        var successCount = 0;
        var conflictCount = 0;

        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            await using var context = new AppDbContext(options);
            var repo = new EfSparePartRepository(context);

            try
            {
                await repo.ReserveAsync(1, channels[i], buyers[i], initialStamp);
                Interlocked.Increment(ref successCount);
            }
            catch (ConcurrencyConflictException)
            {
                Interlocked.Increment(ref conflictCount);
            }
        });

        await Task.WhenAll(tasks);

        // Assert : seule 1 réservation réussit, les 4 autres lèvent ConcurrencyConflictException
        Assert.Equal(1, successCount);
        Assert.Equal(4, conflictCount);
    }

    [Fact]
    public async Task Fix_StockNeverGoesNegative()
    {
        // Arrange : une pièce unique (stock = 1)
        var options = CreateInMemoryOptions();
        string initialStamp;

        await using (var setup = new AppDbContext(options))
        {
            var part = CreateTestPart();
            initialStamp = part.ConcurrencyStamp;
            setup.SpareParts.Add(part);
            await setup.SaveChangesAsync();
        }

        // Act : 5 tâches concurrentes avec le MÊME ConcurrencyStamp initial
        var tasks = Enumerable.Range(0, 5).Select(async i =>
        {
            await using var context = new AppDbContext(options);
            var repo = new EfSparePartRepository(context);

            try
            {
                await repo.ReserveAsync(1, $"channel-{i}", $"buyer-{i}", initialStamp);
            }
            catch (ConcurrencyConflictException)
            {
                // Expected pour les perdants
            }
        });

        await Task.WhenAll(tasks);

        // Assert : stock == 0, jamais négatif
        await using var verify = new AppDbContext(options);
        var finalPart = await verify.SpareParts.FindAsync(1);
        Assert.Equal(0, finalPart!.StockQuantity);
        Assert.Equal(PartStatus.Reserved, finalPart.Status);
    }

    [Fact]
    public async Task Fix_SequentialReservations_DifferentParts_AllSucceed()
    {
        // Arrange : 3 pièces différentes
        var options = CreateInMemoryOptions();
        var stamps = new string[3];

        await using (var setup = new AppDbContext(options))
        {
            for (int i = 1; i <= 3; i++)
            {
                var part = new SparePart
                {
                    Id = i,
                    Reference = $"REF-{i:D3}",
                    Description = $"Pièce test {i}",
                    VhuCenter = $"VHU Test #{i}",
                    Price = 50.00m * i,
                    StockQuantity = 1,
                    Status = PartStatus.Available,
                    CreatedAt = DateTime.UtcNow,
                    ConcurrencyStamp = Guid.NewGuid().ToString()
                };
                stamps[i - 1] = part.ConcurrencyStamp;
                setup.SpareParts.Add(part);
            }
            await setup.SaveChangesAsync();
        }

        // Act : 3 réservations séquentielles sur 3 pièces différentes
        var successCount = 0;

        for (int i = 1; i <= 3; i++)
        {
            await using var context = new AppDbContext(options);
            var repo = new EfSparePartRepository(context);
            var result = await repo.ReserveAsync(i, $"channel-{i}", $"buyer-{i}", stamps[i - 1]);
            if (result is not null) successCount++;
        }

        // Assert : les 3 réservations réussissent (pas de lock global)
        Assert.Equal(3, successCount);

        await using var verify = new AppDbContext(options);
        var allParts = await verify.SpareParts.ToListAsync();
        Assert.All(allParts, p =>
        {
            Assert.Equal(0, p.StockQuantity);
            Assert.Equal(PartStatus.Reserved, p.Status);
        });
    }
}
