using System.Collections.Generic;
using System.Threading.Tasks;
using hikingRepository.Model;
using hikingRepository.Repositories;

namespace hikingService.Services;

public class GearService(GearRepository repo)
{
    public Task<List<GearModel>> GetAllGearsAsync()
    {
        return repo.GetAllGearsAsync();
    }
}