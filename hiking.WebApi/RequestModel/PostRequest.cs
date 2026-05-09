using System;
using System.Collections.Generic;
using hikingService.Dtos;

namespace hiking_controller.RequestModel;

public class CreatePostRequest
{
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public List<GearInputDto> Gears { get; set; } = [];
    public List<Guid> LibraryGearIds { get; set; } = [];
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
    public List<Guid> PhotoIdsToDelete { get; set; } = [];
    public List<GearInputDto> GearsToAdd { get; set; } = [];
    public List<GearInputDto> GearsToUpdate { get; set; } = [];
    public List<Guid> GearIdsToDelete { get; set; } = [];
    public List<Guid> LibraryGearIdsToLink { get; set; } = [];
    public string? DateStart { get; set; }
    public string? DateEnd { get; set; }
    public string? Weather { get; set; }
    public int? PeopleCount { get; set; }
    public List<string> Tags { get; set; } = [];
}