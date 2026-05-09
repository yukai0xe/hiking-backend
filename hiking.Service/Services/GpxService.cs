using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Xml.Linq;
using hikingRepository.Model;
using hikingRepository.Repositories;

namespace hikingService.Services;

public class GpxService(PostRepository postRepo, StorageService storage)
{
    public Task<(string? DateStart, string? DateEnd)> ExtractDatesAsync(Stream stream)
    {
        var doc = XDocument.Load(stream);
        XNamespace ns = "http://www.topografix.com/GPX/1/1";

        var times = doc.Descendants(ns + "time")
            .Select(e => e.Value)
            .Where(v => !string.IsNullOrEmpty(v))
            .ToList();

        if (times.Count == 0) return Task.FromResult<(string?, string?)>((null, null));

        return Task.FromResult<(string?, string?)>(
            (times.First().Split('T')[0], times.Last().Split('T')[0]));
    }

    public async Task UpdateGpxAsync(Guid id, FileData gpxFile)
    {
        var existing = await postRepo.GetByIdAsync(id) ?? throw new KeyNotFoundException();
        var ts = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

        var gpxUrl = existing.GpxFile;
        // var (dateStart, dateEnd) = (cmd.DateStart, cmd.DateEnd);
        if (gpxFile is not null)
        {
            // if (string.IsNullOrEmpty(cmd.DateStart))
            //     (dateStart, dateEnd) = await gpx.ExtractDatesAsync(cmd.GpxFile.Stream);
          
            gpxUrl = await storage.UploadAsync(
                "gpx", $"{id}-{ts}.gpx",
               gpxFile.Stream, gpxFile.ContentType);
        }

        await postRepo.UpdatePostGpxAsync(id, new
        {
            Id = id,
            GpxUrl = gpxUrl
        });
    }
}