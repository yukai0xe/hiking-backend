using Dapper;
using Npgsql;

namespace hikingRepository.Repositories;

public class PhotoRepository(NpgsqlDataSource db)
{
    public async Task DeleteAsync(Guid id)
    {
        await using var conn = await db.OpenConnectionAsync();
        await conn.ExecuteAsync(
            "DELETE FROM photos WHERE id = @id", new { id });
    }
}