using hikingRepository.Model;
using hikingRepository.Repositories;
using hikingService.Commands;
using hikingService.Dtos;

namespace hikingService.Services;

public class PostService(PostRepository repo, StorageService storage, GpxService gpx, PhotoService photo)
{
  public async Task<Guid> CreateAsync(CreatePostCommand cmd)
  {
      var id = Guid.NewGuid();
      var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        
      var compressedBytes = await photo.CompressAsync(cmd.CoverFile.Stream);

      var compressedCoverUrl = await storage.UploadAsync(
          "covers", $"{id}-{ts}.jpg",
          compressedBytes, "image/jpeg");
      
      cmd.CoverFile.Stream.Position = 0;
      var coverUrl = await storage.UploadAsync(
          "covers", $"{id}-{ts}{Ext(cmd.CoverFile.FileName)}",
          cmd.CoverFile.Stream, cmd.CoverFile.ContentType);
      

      var gpxUrl = "";
      var (dateStart, dateEnd) = (cmd.DateStart, cmd.DateEnd);
      if (cmd.GpxFile is not null)
      {
          if (string.IsNullOrEmpty(dateStart))
              (dateStart, dateEnd) = await gpx.ExtractDatesAsync(cmd.GpxFile.Stream);
          
          gpxUrl = await storage.UploadAsync(
              "gpx", $"{id}-{ts}.gpx",
              cmd.GpxFile.Stream, cmd.GpxFile.ContentType);
      }

      await repo.InsertPostAsync(new PostModel()
      {
          Id          = id,
          Title       = cmd.Title,
          Description = cmd.Description,
          CoverImage  = coverUrl,
          CompressedCoverImage = compressedCoverUrl,
          GpxFile     = gpxUrl,
          DateStart   = dateStart,
          DateEnd     = dateEnd,
          Weather     = cmd.Weather,
          PeopleCount = cmd.PeopleCount,
          Tags        = [.. cmd.Tags],
          CreatedAt   = DateTime.UtcNow,
      });

      if (cmd.PhotoFiles.Count > 0)
      {
          var urls = await Task.WhenAll(cmd.PhotoFiles.Select((f, i) =>
              storage.UploadAsync("photos", $"{id}-{ts}-{i}{Ext(f.FileName)}", f.Stream, f.ContentType)));
          await repo.InsertPhotosAsync(urls.Select(u =>
              new PhotoModel { Id = Guid.NewGuid(), PostId = id, Url = u, CreatedAt = DateTime.UtcNow }).ToList());
      }

      if (cmd.Gears.Count > 0)
          await repo.InsertGearsAsync(cmd.Gears.Select(g =>
              new GearModel { Id = Guid.NewGuid(), PostId = id, Name = g.Name, Weight = g.Weight, Note = g.Note }).ToList());

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
          await repo.InsertGearsAsync(cmd.GearsToAdd.Select(g =>
              new GearModel { Id = Guid.NewGuid(), PostId = id, Name = g.Name, Weight = g.Weight, Note = g.Note }
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
  
  // ... GetAllAsync, GetDetailAsync, DeleteAsync 同前

  private static string Ext(string fileName) => Path.GetExtension(fileName);
}