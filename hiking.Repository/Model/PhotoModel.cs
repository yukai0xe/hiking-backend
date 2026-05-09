using System;

namespace hikingRepository.Model;

public class PhotoModel
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public string Url { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}