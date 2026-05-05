using Dapper;
using hikingRepository.Model;
using Npgsql;

namespace hikingRepository.Repositories;

 public class PostRepository(NpgsqlDataSource db)
  {
      public async Task<List<PostModel>> GetAllAsync()
      {
          await using var conn = await db.OpenConnectionAsync();
          return (await conn.QueryAsync<PostModel>("""
              SELECT * FROM posts
              WHERE deleted_at IS NULL
              ORDER BY created_at DESC
              """)).ToList();
      }

      public async Task<PostModel?> GetByIdAsync(Guid id)
      {
          await using var conn = await db.OpenConnectionAsync();
          return await conn.QuerySingleOrDefaultAsync<PostModel>("""
              SELECT * FROM posts
              WHERE id = @id AND deleted_at IS NULL
              """, new { id });
      }

      public async Task<List<PhotoModel>> GetPhotosAsync(Guid postId)
      {
          await using var conn = await db.OpenConnectionAsync();
          return (await conn.QueryAsync<PhotoModel>("""
              SELECT * FROM photos
              WHERE post_id = @postId
              ORDER BY created_at
              """, new { postId })).ToList();
      }

      public async Task<List<GearModel>> GetGearsAsync(Guid postId)
      {
          await using var conn = await db.OpenConnectionAsync();
          return (await conn.QueryAsync<GearModel>("""
              SELECT * FROM gears WHERE post_id = @postId
              """, new { postId })).ToList();
      }

      public async Task InsertPostAsync(PostModel post)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              INSERT INTO posts
                (id, title, description, cover_image, gpx_file,
                 date_start, date_end, weather, people_count, tags, created_at)
              VALUES
                (@Id, @Title, @Description, @CoverImage, @GpxFile,
                 @DateStart, @DateEnd, @Weather, @PeopleCount, @Tags, @CreatedAt)
              """, post);
      }

      public async Task InsertPhotosAsync(List<PhotoModel> photos)
      {
          if (photos.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              INSERT INTO photos (id, post_id, url, created_at)
              VALUES (@Id, @PostId, @Url, @CreatedAt)
              """, photos);
      }

      public async Task InsertGearsAsync(List<GearModel> gears)
      {
          if (gears.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              INSERT INTO gears (id, post_id, name, weight, note)
              VALUES (@Id, @PostId, @Name, @Weight, @Note)
              """, gears);
      }

      public async Task UpdatePostAsync(Guid id, object updates)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET
                title        = @Title,
                description  = @Description,
                cover_image  = @CoverImage,
                gpx_file     = @GpxFile,
                date_start   = @DateStart,
                date_end     = @DateEnd,
                weather      = @Weather,
                people_count = @PeopleCount,
                tags         = @Tags
              WHERE id = @Id
              """, updates);
      }

      public async Task DeletePhotosAsync(List<Guid> ids)
      {
          if (ids.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              DELETE FROM photos WHERE id = ANY(@ids)
              """, new { ids = ids.ToArray() });
      }

      public async Task SoftDeletePostAsync(Guid id)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET deleted_at = @now WHERE id = @id
              """, new { id, now = DateTime.UtcNow });
      }
  }