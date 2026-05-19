using System;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using hiking_controller.RequestModel;
using hikingService.Commands;
using hikingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController(PostService svc, PdfExportService pdfSvc) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll() =>
        Ok(await svc.GetAllAsync());

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> GetDetail(Guid id)
    {
        var detail = await svc.GetDetailAsync(id);
        return detail is null ? NotFound() : Ok(detail);
    }

    [HttpPost]
    [Consumes("application/json")]
    public async Task<IActionResult> Create([FromBody] CreatePostRequest req)
    {
        var cmd = new CreatePostCommand
        {
            Title          = req.Title,
            Description    = req.Description,
            Gears          = req.Gears,
            LibraryGearIds = req.LibraryGearIds,
            DateStart      = req.DateStart,
            DateEnd        = req.DateEnd,
            Weather        = req.Weather,
            PeopleCount    = req.PeopleCount,
            Tags           = req.Tags,
        };

        var id = await svc.CreateAsync(cmd);
        return CreatedAtAction(nameof(GetDetail), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Consumes("application/json")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdatePostRequest req)
    {
        var cmd = new UpdatePostCommand()
        {
            Title = req.Title,
            Description = req.Description,
            PhotoIdsToDelete = req.PhotoIdsToDelete,
            GearsToAdd = req.GearsToAdd,
            GearsToUpdate = req.GearsToUpdate,
            GearIdsToDelete = req.GearIdsToDelete,
            LibraryGearIdsToLink = req.LibraryGearIdsToLink,
            DateStart = req.DateStart,
            DateEnd = req.DateEnd,
            Weather = req.Weather,
            PeopleCount = req.PeopleCount,
            Tags = req.Tags,
        };
        await svc.UpdateAsync(id, cmd);
        return NoContent();
    }
    

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }

    [HttpGet("{id:guid}/export/pdf")]
    public async Task<IActionResult> ExportPdf(Guid id, [FromQuery] bool includeGears = true)
    {
        var detail = await svc.GetDetailAsync(id);
        if (detail is null) return NotFound();

        var bytes    = pdfSvc.Generate(detail, includeGears);
        var filename = SanitizeFilename(detail.Post.Title) + ".pdf";
        return File(bytes, "application/pdf", filename);
    }

    private static string SanitizeFilename(string s) =>
        Regex.Replace(s, @"[/\\?%*:|""<>]", "-").Trim();
}