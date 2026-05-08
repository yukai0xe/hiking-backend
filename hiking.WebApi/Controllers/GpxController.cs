using hikingRepository.Model;
using hikingService;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GpxController(GpxService svc) : ControllerBase
{
    
    [HttpPost("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> UpdateGpx(Guid id, IFormFile gpxFile)
    {
        await svc.UpdateGpxAsync(id, ToFileData(gpxFile));
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