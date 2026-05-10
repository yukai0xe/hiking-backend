using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using hikingRepository.Model;
using Npgsql;

namespace hikingRepository.Repositories;

public class GearRepository(NpgsqlDataSource db)
{
    public async Task<List<GearModel>> GetAllGearsAsync()
    {
        await using var conn = await db.OpenConnectionAsync();
        return (await conn.QueryAsync<GearModel>(
                "SELECT * FROM gears WHERE deleted_at IS NULL ORDER BY name"))
            .ToList();
    }

    public async Task<Guid> CreateAsync(GearModel gear)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync("""
            INSERT INTO gears (id, name, weight, note, category, quantity, brand, reference_url, price, added_at)
            VALUES (@Id, @Name, @Weight, @Note, @Category, @Quantity, @Brand, @ReferenceUrl, @Price, @AddedAt)
            """, gear);
        return gear.Id;
    }

    public async Task UpdateAsync(GearModel gear)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync("""
            UPDATE gears SET
                name          = @Name,
                weight        = @Weight,
                note          = @Note,
                category      = @Category,
                quantity      = @Quantity,
                brand         = @Brand,
                reference_url = @ReferenceUrl,
                price         = @Price,
                added_at      = @AddedAt
            WHERE id = @Id AND deleted_at IS NULL
            """, gear);
    }

    public async Task SoftDeleteAsync(Guid id)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "UPDATE gears SET deleted_at = @now WHERE id = @id",
            new { id, now = DateTime.UtcNow });
    }

    public async Task<List<string>> GetCategoriesAsync()
    {
        await using var conn = await db.OpenConnectionAsync();
        return (await conn.QueryAsync<string>(
            "SELECT name FROM gear_categories ORDER BY name"))
            .ToList();
    }
}
