using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

      public async Task<List<GearDetailModel>> GetGearsAsync(Guid postId)
      {
          await using var conn = await db.OpenConnectionAsync();
          return (await conn.QueryAsync<GearDetailModel>("""
              SELECT
                  gmp.id,
                  gmp.post_id,
                  gmp.gear_id,
                  gmp.weight,
                  gmp.quantity,
                  g.name,
                  g.note,
                  g.category,
                  g.brand,
                  g.reference_url,
                  g.price,
                  g.added_at
              FROM gears_mapping_post gmp
              JOIN gears g ON g.id = gmp.gear_id
              WHERE gmp.post_id = @postId
              """, new { postId })).ToList();
      }

      public async Task InsertPostAsync(PostModel post)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              INSERT INTO posts
                (id, title, description, cover_image, compressed_cover_image, gpx_file,
                 date_start, date_end, weather, people_count, tags, created_at)
              VALUES
                (@Id, @Title, @Description, @CoverImage, @CompressedCoverImage, @GpxFile,
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

      // Inserts a new gear into the library, then creates a mapping row for the post
      public async Task InsertGearsAsync(List<GearMappingModel> mappings, List<GearModel> gears)
      {
          if (gears.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              INSERT INTO gears (id, name, weight, note, category, quantity, brand, reference_url, price, added_at)
              VALUES (@Id, @Name, @Weight, @Note, @Category, @Quantity, @Brand, @ReferenceUrl, @Price, @AddedAt)
              ON CONFLICT (id) DO NOTHING
              """, gears);
          await conn.ExecuteAsync("""
              INSERT INTO gears_mapping_post (id, post_id, gear_id, weight, quantity)
              VALUES (@Id, @PostId, @GearId, @Weight, @Quantity)
              """, mappings);
      }

      // Links existing library gears to a post without creating new gear entries.
      // Weight/quantity are copied from the gear library row as defaults for the mapping.
      public async Task InsertGearMappingsAsync(Guid postId, List<Guid> gearIds)
      {
          if (gearIds.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          // Pass UUIDs as text[] and cast in SQL; Dapper cannot infer uuid[] from Guid[]
          // causing ANY(@GearIds) to silently match 0 rows when bound as an anonymous object.
          await conn.ExecuteAsync("""
              INSERT INTO gears_mapping_post (id, post_id, gear_id, weight, quantity)
              SELECT gen_random_uuid(), @PostId, g.id, g.weight, g.quantity
              FROM gears g
              WHERE g.id = ANY(@GearIds::uuid[])
              """, new { PostId = postId, GearIds = gearIds.Select(g => g.ToString()).ToArray() });
      }

      // Updates gear library attributes AND the post-specific weight/quantity in the mapping row
      // @Id is the mapping ID; gear library row is resolved via the mapping table
      public async Task UpdateGearsAsync(List<GearDetailModel> gears)
      {
          if (gears.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE gears g
              SET name          = @Name,
                  note          = @Note,
                  category      = @Category,
                  brand         = @Brand,
                  reference_url = @ReferenceUrl,
                  price         = @Price,
                  added_at      = @AddedAt,
                  weight        = @Weight,
                  quantity      = @Quantity
              FROM gears_mapping_post gmp
              WHERE gmp.id = @Id AND g.id = gmp.gear_id
              """, gears);
          await conn.ExecuteAsync("""
              UPDATE gears_mapping_post
              SET weight   = @Weight,
                  quantity = @Quantity
              WHERE id = @Id
              """, gears);
      }

      // Deletes mapping rows only; gear library entries are preserved
      public async Task DeleteGearsAsync(List<Guid> mappingIds)
      {
          if (mappingIds.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync(
              "DELETE FROM gears_mapping_post WHERE id = ANY(@Ids::uuid[])",
              new { Ids = mappingIds.Select(g => g.ToString()).ToArray() });
      }

      public async Task UpdatePostAsync(Guid id, object updates)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET
                title        = @Title,
                description  = @Description,
                date_start   = @DateStart,
                date_end     = @DateEnd,
                weather      = @Weather,
                people_count = @PeopleCount,
                tags         = @Tags
              WHERE id = @Id
              """, updates);
      }

      public async Task UpdatePostCoverAsync(Guid id, object coverImages)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET
                cover_image            = @CoverImage,
                compressed_cover_image = @CompressedCoverImage
              WHERE id = @Id
              """, coverImages);
      }

      public async Task UpdatePostGpxAsync(Guid id, object gpx)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET
                gpx_file = @GpxUrl
              WHERE id = @Id
              """, gpx);
      }

      public async Task DeletePhotosAsync(List<Guid> ids)
      {
          if (ids.Count == 0) return;
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync(
              "DELETE FROM photos WHERE id = ANY(@Ids::uuid[])",
              new { Ids = ids.Select(g => g.ToString()).ToArray() });
      }

      public async Task SoftDeletePostAsync(Guid id)
      {
          await using var conn = await db.OpenConnectionAsync();
          await conn.ExecuteAsync("""
              UPDATE posts SET deleted_at = @now WHERE id = @id
              """, new { id, now = DateTime.UtcNow });
      }
  }