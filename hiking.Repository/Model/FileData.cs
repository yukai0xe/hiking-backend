using System.IO;

namespace hikingRepository.Model;

public record FileData(Stream Stream, string FileName, string ContentType);