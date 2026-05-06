using hikingRepository.Repositories;

namespace hikingService.Services;

public class TagService(TagRepository repo)
{
    public Task<List<string>> GetAllAsync() => repo.GetAllAsync();
    public Task CreateAsync(string name)    => repo.InsertAsync(name.Trim());
}