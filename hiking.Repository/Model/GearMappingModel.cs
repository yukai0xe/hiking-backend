using System;

namespace hikingRepository.Model;

public class GearMappingModel
{
    public Guid Id { get; set; }
    public Guid PostId { get; set; }
    public Guid GearId { get; set; }
    public int Weight { get; set; }
    public int Quantity { get; set; } = 1;
}
