using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using hikingRepository.Model;
using hikingRepository.Repositories;
using hikingService.Commands;
using hikingService.Dtos;

namespace hikingService.Services;

public class PostService(PostRepository repo)
{
  public async Task<Guid> CreateAsync(CreatePostCommand cmd)
  {
      var id = Guid.NewGuid();

      await repo.InsertPostAsync(new PostModel
      {
          Id          = id,
          Title       = cmd.Title,
          Description = cmd.Description,
          CoverImage  = "",
          CompressedCoverImage = "",
          GpxFile     = "",
          DateStart   = cmd.DateStart,
          DateEnd     = cmd.DateEnd,
          Weather     = cmd.Weather,
          PeopleCount = cmd.PeopleCount,
          Tags        = [.. cmd.Tags],
          CreatedAt   = DateTime.UtcNow,
      });

      if (cmd.Gears.Count > 0)
      {
          var gearModels = cmd.Gears.Select(g => new GearModel
          {
              Id           = Guid.NewGuid(),
              Name         = g.Name,
              Weight       = g.Weight,
              Note         = g.Note,
              Category     = g.Category,
              Quantity     = g.Quantity,
              Brand        = g.Brand,
              Price        = g.Price,
              ReferenceUrl = g.ReferenceUrl,
              AddedAt      = g.AddedAt,
          }).ToList();
          var mappings = gearModels.Select(g => new GearMappingModel
          {
              Id       = Guid.NewGuid(),
              PostId   = id,
              GearId   = g.Id,
              Weight   = g.Weight,
              Quantity = g.Quantity,
          }).ToList();
          await repo.InsertGearsAsync(mappings, gearModels);
      }

      if (cmd.LibraryGearIds.Count > 0)
          await repo.InsertGearMappingsAsync(id, cmd.LibraryGearIds);

      return id;
  }

  public async Task UpdateAsync(Guid id, UpdatePostCommand cmd)
  {
      var existing = await repo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
      var (dateStart, dateEnd) = (cmd.DateStart, cmd.DateEnd);
      await repo.UpdatePostAsync(id, new
      {
          Id          = id,
          Title       = cmd.Title,
          Description = cmd.Description,
          DateStart   = dateStart,
          DateEnd     = dateEnd,
          Weather     = cmd.Weather,
          PeopleCount = cmd.PeopleCount,
          Tags        = cmd.Tags.ToArray(),
      });

      await repo.DeletePhotosAsync(cmd.PhotoIdsToDelete);
      await repo.DeleteGearsAsync(cmd.GearIdsToDelete);

      if (cmd.GearsToAdd.Count > 0)
      {
          var gearModels = cmd.GearsToAdd.Select(g => new GearModel
          {
              Id           = Guid.NewGuid(),
              Name         = g.Name,
              Weight       = g.Weight,
              Note         = g.Note,
              Category     = g.Category,
              Quantity     = g.Quantity,
              Brand        = g.Brand,
              Price        = g.Price,
              ReferenceUrl = g.ReferenceUrl,
              AddedAt      = g.AddedAt,
          }).ToList();
          var mappings = gearModels.Select(g => new GearMappingModel
          {
              Id       = Guid.NewGuid(),
              PostId   = id,
              GearId   = g.Id,
              Weight   = g.Weight,
              Quantity = g.Quantity,
          }).ToList();
          await repo.InsertGearsAsync(mappings, gearModels);
      }

      if (cmd.LibraryGearIdsToLink.Count > 0)
          await repo.InsertGearMappingsAsync(id, cmd.LibraryGearIdsToLink);

      if (cmd.GearsToUpdate.Count > 0)
      {
          await repo.UpdateGearsAsync(cmd.GearsToUpdate.Select(g =>
              new GearDetailModel
              {
                  Id           = g.Id,   // mapping ID — SQL resolves gear_id from mapping table
                  PostId       = id,
                  Name         = g.Name,
                  Weight       = g.Weight,
                  Note         = g.Note,
                  Category     = g.Category,
                  Quantity     = g.Quantity,
                  Brand        = g.Brand,
                  Price        = g.Price,
                  ReferenceUrl = g.ReferenceUrl,
                  AddedAt      = g.AddedAt,
              }
          ).ToList());
      }
  }
  
  public Task<List<PostModel>> GetAllAsync() => repo.GetAllAsync();

  public async Task<PostDetailDto?> GetDetailAsync(Guid id)
  {
      var post = await repo.GetByIdAsync(id);
      if (post is null) return null;

      var photosTask = repo.GetPhotosAsync(id);
      var gearsTask  = repo.GetGearsAsync(id);
      await Task.WhenAll(photosTask, gearsTask);

      return new PostDetailDto
      {
          Post   = post,
          Photos = await photosTask,
          Gears  = await gearsTask,
      };
  }

  public Task DeleteAsync(Guid id) => repo.SoftDeletePostAsync(id);
}