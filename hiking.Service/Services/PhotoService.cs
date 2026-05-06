using hikingRepository.Repositories;

namespace hikingService.Services;

public class PhotoService(PhotoRepository repo)
{
    public Task DeleteAsync(Guid id) => repo.DeleteAsync(id);
}