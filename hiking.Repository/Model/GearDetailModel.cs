using System;

namespace hikingRepository.Model;

// Flat JOIN result of gears_mapping_post + gears, returned for post detail
public class GearDetailModel
{
    public Guid Id { get; set; }       // mapping row ID (used by frontend for edit/delete)
    public Guid GearId { get; set; }   // gear library ID
    public Guid PostId { get; set; }
    public string Name { get; set; } = "";
    public int Weight { get; set; }    // from mapping (post-specific)
    public string Note { get; set; } = "";
    public string Category { get; set; } = "其他";
    public int Quantity { get; set; } = 1;  // from mapping (post-specific)
    public string? Brand { get; set; }
    public string? ReferenceUrl { get; set; }
    public int? Price { get; set; }
    public string? AddedAt { get; set; }
}
