using System.Collections.Generic;
using hikingRepository.Model;

namespace hikingService.Dtos;

public class PostDetailDto
{
    public PostModel Post { get; set; } = null!;
    public List<PhotoModel> Photos { get; set; } = [];
    public List<GearDetailModel> Gears { get; set; } = [];
}