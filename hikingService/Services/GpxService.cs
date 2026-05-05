using System.Xml.Linq;

namespace hikingService;

public class GpxService
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
}