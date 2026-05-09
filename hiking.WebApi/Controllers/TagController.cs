using System.Threading.Tasks;
using hiking_controller.RequestModel;
using hikingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController(TagService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() => Ok(await svc.GetAllAsync());

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTagRequest req)
    {
        await svc.CreateAsync(req.Name);
        return NoContent();
    }
}