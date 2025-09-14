using Meow.Api.Data;
using Meow.Shared.Dtos.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AvatarsController : ControllerBase
{
    private readonly AppDbContext _db;

    public AvatarsController(AppDbContext db)
    {
        _db = db;
    }

    // GET: /api/Avatars
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AvatarDto>>> GetAll()
    {
        var avatars = await _db.AvatarCatalogs
            .AsNoTracking()
            .Where(a => a.Status == "Active")
            .OrderBy(a => a.Name)
            .Select(a => new AvatarDto
            {
                AvatarID = a.AvatarID,
                Name = a.Name,
                ImageUrl = a.ImageUrl,
                Category = a.Category,
                Tags = a.Tags
            })
            .ToListAsync();

        return Ok(avatars);
    }
}
