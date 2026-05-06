using Dapper;
using Npgsql;

namespace hikingRepository.Repositories;

public class TagRepository(NpgsqlDataSource db)
{
    public async Task<List<string>> GetAllAsync()
    {
        await using var conn = await db.OpenConnectionAsync();
        return (await conn.QueryAsync<string>(
            "SELECT name FROM tags ORDER BY name")).ToList();
    }

    public async Task InsertAsync(string name)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "INSERT INTO tags (name) VALUES (@name) ON CONFLICT (name) DO NOTHING",
            new { name });
    }
}