using hikingService.Services;
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
}