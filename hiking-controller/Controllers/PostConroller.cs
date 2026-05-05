using hiking_controller.RequestModel;
using hikingRepository.Model;
using hikingService.Commands;
using hikingService.Services;
using Microsoft.AspNetCore.Mvc;

namespace hiking_controller.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PostsController(PostService svc) : ControllerBase
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
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Create([FromForm] CreatePostRequest req)
    {
        var cmd = new CreatePostCommand
        {
            Title       = req.Title,
            Description = req.Description,
            CoverFile   = ToFileData(req.CoverFile),
            GpxFile     = req.GpxFile is null ? null : ToFileData(req.GpxFile),
            PhotoFiles  = req.PhotoFiles.Select(ToFileData).ToList(),
            Gears       = req.Gears,
            DateStart   = req.DateStart,
            DateEnd     = req.DateEnd,
            Weather     = req.Weather,
            PeopleCount = req.PeopleCount,
            Tags        = req.Tags,
        };

        var id = await svc.CreateAsync(cmd);
        return CreatedAtAction(nameof(GetDetail), new { id }, new { id });
    }

    [HttpPut("{id:guid}")]
    [Consumes("multipart/form-data")]
    public async Task<IActionResult> Update(Guid id, [FromForm] UpdatePostRequest req)
    {
        var cmd = new UpdatePostCommand
        {
            Title            = req.Title,
            Description      = req.Description,
            CoverFile        = req.CoverFile is null ? null : ToFileData(req.CoverFile),
            GpxFile          = req.GpxFile is null ? null : ToFileData(req.GpxFile),
            PhotoFilesToAdd  = req.PhotoFilesToAdd.Select(ToFileData).ToList(),
            PhotoIdsToDelete = req.PhotoIdsToDelete,
            DateStart        = req.DateStart,
            DateEnd          = req.DateEnd,
            Weather          = req.Weather,
            PeopleCount      = req.PeopleCount,
            Tags             = req.Tags,
        };

        await svc.UpdateAsync(id, cmd);
        return NoContent();
    }

    private static FileData ToFileData(IFormFile f) =>
        new(f.OpenReadStream(), f.FileName, f.ContentType);

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id)
    {
        await svc.DeleteAsync(id);
        return NoContent();
    }
}