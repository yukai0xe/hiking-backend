using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using hikingRepository.Model;
using hikingRepository.Repositories;
using hikingService.Dtos;

namespace hikingService.Services;

public class GearService(GearRepository repo)
{
    public Task<List<GearModel>> GetAllGearsAsync() => repo.GetAllGearsAsync();

    public async Task<Guid> CreateAsync(GearInputDto dto)
    {
        var gear = new GearModel
        {
            Id           = Guid.NewGuid(),
            Name         = dto.Name,
            Weight       = dto.Weight,
            Note         = dto.Note,
            Category     = dto.Category,
            Quantity     = dto.Quantity,
            Brand        = dto.Brand,
            ReferenceUrl = dto.ReferenceUrl,
            Price        = dto.Price,
            AddedAt      = dto.AddedAt,
        };
        return await repo.CreateAsync(gear);
    }

    public async Task UpdateAsync(Guid id, GearInputDto dto)
    {
        var gear = new GearModel
        {
            Id           = id,
            Name         = dto.Name,
            Weight       = dto.Weight,
            Note         = dto.Note,
            Category     = dto.Category,
            Quantity     = dto.Quantity,
            Brand        = dto.Brand,
            ReferenceUrl = dto.ReferenceUrl,
            Price        = dto.Price,
            AddedAt      = dto.AddedAt,
        };
        await repo.UpdateAsync(gear);
    }

    public Task DeleteAsync(Guid id) => repo.SoftDeleteAsync(id);

    public Task<List<string>> GetCategoriesAsync() => repo.GetCategoriesAsync();
}
