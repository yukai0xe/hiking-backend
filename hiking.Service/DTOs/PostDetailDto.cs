using hikingRepository.Model;

namespace hikingService.Dtos;

public class PostDetailDto
{
    public PostModel Post { get; set; } = null!;
    public List<PhotoModel> Photos { get; set; } = [];
    public List<GearModel> Gears { get; set; } = [];
}