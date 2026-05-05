namespace hikingRepository.Model;

public class PostModel
{
    public Guid Id { get; set; }
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public string CoverImage { get; set; } = "";
    public string GpxFile { get; set; } = "";
    public DateTime CreatedAt { get; set; }
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public DateTime? DeletedAt { get; set; }
    public string[] Tags { get; set; } = [];
}