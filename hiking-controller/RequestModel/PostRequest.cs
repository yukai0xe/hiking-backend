using hikingService.Dtos;

namespace hiking_controller.RequestModel;

public class CreatePostRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public IFormFile CoverFile { get; set; } = null!;
    public IFormFile? GpxFile { get; set; }
    public List<IFormFile> PhotoFiles { get; set; } = [];
    public List<GearInputDto> Gears { get; set; } = [];
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class UpdatePostRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public IFormFile? CoverFile { get; set; }
    public IFormFile? GpxFile { get; set; }
    public List<IFormFile> PhotoFilesToAdd { get; set; } = [];
    public List<Guid> PhotoIdsToDelete { get; set; } = [];
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public List<string> Tags { get; set; } = [];
}