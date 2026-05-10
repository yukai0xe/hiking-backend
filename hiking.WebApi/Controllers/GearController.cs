using System;
using System.Threading.Tasks;
using hikingService.Dtos;
using hikingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GearsController(GearService svc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await svc.GetAllGearsAsync());

    [HttpGet("categories")]
    public async Task<IActionResult> GetCategories() =>
        Ok(await svc.GetCategoriesAsync());

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] GearInputDto dto)
    {
        var id = await svc.CreateAsync(dto);
        return CreatedAtAction(nameof(GetAll), new { }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(Guid id, [FromBody] GearInputDto dto)
    {
        await svc.UpdateAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }
}
