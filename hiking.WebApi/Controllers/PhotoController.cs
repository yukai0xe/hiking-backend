using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using hikingRepository.Model;
using hikingService.Services;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PhotosController(PhotoService svc) : ControllerBase
{
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }
    
    [HttpPost("{id:guid}/cover")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateCover(Guid id, IFormFile coverFile)
    {
        await svc.UpdateCoverAsync(id, ToFileData(coverFile));
        return NoContent();
    }
    
    [HttpPost("{id:guid}/photos")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> AddPhotos(Guid id, List<IFormFile> photos)
    {
        await svc.AddPhotosAsync(id, photos.Select(ToFileData).ToList());
        return NoContent();
    }
    
    private static FileData ToFileData(IFormFile f)
    {
        var ms = new MemoryStream((int)f.Length);
        f.CopyTo(ms);
        ms.Position = 0;
        return new FileData(ms, f.FileName, f.ContentType);
    }
}