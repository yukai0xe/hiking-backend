using hikingRepository.Model;
using hikingService.Dtos;

namespace hikingService.Commands;

public class CreatePostCommand
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public FileData CoverFile { get; set; } = null!;
    public FileData? GpxFile { get; set; }
    public List<FileData> PhotoFiles { get; set; } = [];
    public List<GearInputDto> Gears { get; set; } = [];
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public List<string> Tags { get; set; } = [];
}

public class UpdatePostCommand
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public FileData? CoverFile { get; set; }
    public FileData? GpxFile { get; set; }
    public List<FileData> PhotoFilesToAdd { get; set; } = [];
    public List<Guid> PhotoIdsToDelete { get; set; } = [];
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public List<string> Tags { get; set; } = [];
}