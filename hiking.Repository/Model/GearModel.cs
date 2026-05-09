using System;

namespace hikingRepository.Model;

public class GearModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = "";
    public int Weight { get; set; }
    public string Note { get; set; } = "";
    public string Category { get; set; } = "其他";
    public int Quantity { get; set; } = 1;
    public string? Brand { get; set; }
    public string? ReferenceUrl { get; set; }
    public int? Price { get; set; }
    public string? AddedAt { get; set; }
    public DateTime? DeletedAt { get; set; }
}