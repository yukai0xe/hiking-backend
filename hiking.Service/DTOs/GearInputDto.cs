using System;

namespace hikingService.Dtos;

public class GearInputDto
{
    public Guid Id { get; set; }   // mapping ID for update/delete; unused for add
    public string Name { get; set; } = "";
    public int Weight { get; set; }
    public string Note { get; set; } = "";
    public string Category { get; set; } = "其他";
    public int Quantity { get; set; } = 1;
    public string? Brand { get; set; }
    public string? ReferenceUrl { get; set; }
    public int? Price { get; set; }
    public string? AddedAt { get; set; }
}