using System.Diagnostics;
using hikingRepository.Model;
using hikingRepository.Repositories;

namespace hikingService.Services;

public class PhotoService(PhotoRepository repo, PostRepository postRepo, StorageService storage)
{
    public Task DeleteAsync(Guid id) => repo.DeleteAsync(id);

    public async Task UpdateCoverAsync(Guid id, FileData coverFile)
    {
        var existing = await postRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var coverImage = existing.CoverImage;
        var compressedCoverUrl = existing.CompressedCoverImage;
        if (coverFile is not null)
        {
            var compressedBytes = await CompressAsync(coverFile.Stream);
          
            compressedCoverUrl = await storage.UploadAsync(
                "covers", $"{id}-{ts}-thumb.jpg",
                compressedBytes, "image/jpeg");
          
            coverFile.Stream.Position = 0;
            coverImage = await storage.UploadAsync(
                "covers", $"{id}-{ts}{Ext(coverFile.FileName)}",
                coverFile.Stream, coverFile.ContentType);
            
            await postRepo.UpdatePostCoverAsync(id, new
            {
                Id = id,
                CompressedCoverImage = compressedCoverUrl,
                CoverImage = coverImage,
            });
        }
    }

    public async Task AddPhotosAsync(Guid id, List<FileData> photoFiles)
    {
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        if (photoFiles.Count > 0)
        {
            var urls = await Task.WhenAll(photoFiles.Select((f, i) =>
                storage.UploadAsync("photos", $"{id}-{ts}-add{i}{Ext(f.FileName)}", f.Stream, f.ContentType)));
            await postRepo.InsertPhotosAsync(urls.Select(u =>
                new PhotoModel { Id = Guid.NewGuid(), PostId = id, Url = u, CreatedAt = DateTime.UtcNow }).ToList());
        }
    }
    
    public async Task<byte[]> CompressAsync(Stream input)
    {
        var inPath  = Path.GetTempFileName() + ".jpg";
        var outPath = Path.GetTempFileName() + ".jpg";

        try
        {
            if (input.CanSeek) input.Position = 0;
            await using (var fs = File.Create(inPath))
                await input.CopyToAsync(fs);

            using var proc = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName               = "ffmpeg",
                    Arguments              = $"-y -i \"{inPath}\" -vf scale=20:-1 \"{outPath}\"",
                    RedirectStandardError  = true,
                    UseShellExecute        = false,
                    CreateNoWindow         = true,
                }
            };

            proc.Start();
            await proc.WaitForExitAsync();

            if (proc.ExitCode != 0)
            {
                var err = await proc.StandardError.ReadToEndAsync();
                throw new InvalidOperationException($"ffmpeg failed: {err}");
            }

            return await File.ReadAllBytesAsync(outPath);
        }
        finally
        {
            File.Delete(inPath);
            File.Delete(outPath);
        }
    }
    
    private static string Ext(string fileName) => Path.GetExtension(fileName);
}