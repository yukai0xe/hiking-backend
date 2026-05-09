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
                "SELECT * FROM gears"))
            .ToList();
    }
}