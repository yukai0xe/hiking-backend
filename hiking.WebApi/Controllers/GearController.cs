using System.Threading.Tasks;
using hikingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GearsController(GearService svc): ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAllGears() => 
        Ok(await svc.GetAllGearsAsync());
}