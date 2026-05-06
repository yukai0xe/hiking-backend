using hikingRepository.Model;
using hikingRepository.Repositories;
using hikingService.Commands;
using hikingService.Dtos;

namespace hikingService.Services;

public class PostService(PostRepository repo, StorageService storage, GpxService gpx)
{
  public async Task<Guid> CreateAsync(CreatePostCommand cmd)
  {
      var id = Guid.NewGuid();
      var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

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
      var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

      var coverImage = existing.CoverImage;
      if (cmd.CoverFile is not null)
          coverImage = await storage.UploadAsync(
              "covers", $"{id}-{ts}{Ext(cmd.CoverFile.FileName)}",
              cmd.CoverFile.Stream, cmd.CoverFile.ContentType);

      var gpxFile = existing.GpxFile;
      var (dateStart, dateEnd) = (cmd.DateStart, cmd.DateEnd);
      if (cmd.GpxFile is not null)
      {
          if (string.IsNullOrEmpty(cmd.DateStart))
              (dateStart, dateEnd) = await gpx.ExtractDatesAsync(cmd.GpxFile.Stream);
          
          gpxFile = await storage.UploadAsync(
              "gpx", $"{id}-{ts}.gpx",
              cmd.GpxFile.Stream, cmd.GpxFile.ContentType);
      }

      await repo.UpdatePostAsync(id, new
      {
          Id          = id,
          Title       = cmd.Title,
          Description = cmd.Description,
          CoverImage  = coverImage,
          GpxFile     = gpxFile,
          DateStart   = dateStart,
          DateEnd     = dateEnd,
          Weather     = cmd.Weather,
          PeopleCount = cmd.PeopleCount,
          Tags        = cmd.Tags.ToArray(),
      });

      await repo.DeletePhotosAsync(cmd.PhotoIdsToDelete);

      if (cmd.PhotoFilesToAdd.Count > 0)
      {
          var urls = await Task.WhenAll(cmd.PhotoFilesToAdd.Select((f, i) =>
              storage.UploadAsync("photos", $"{id}-{ts}-add{i}{Ext(f.FileName)}", f.Stream, f.ContentType)));
          await repo.InsertPhotosAsync(urls.Select(u =>
              new PhotoModel { Id = Guid.NewGuid(), PostId = id, Url = u, CreatedAt = DateTime.UtcNow }).ToList());
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