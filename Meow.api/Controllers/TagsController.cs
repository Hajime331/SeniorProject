using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.Tags;  // Scaffold 產生的 DbContext 和 Entity 類別所在命名空間

namespace Meow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TagsController : ControllerBase
    {
        private readonly AppDbContext _db;

        public TagsController(AppDbContext db)
        {
            _db = db;
        }

        // GET /api/Tags?keyword=
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TagDto>>> Get([FromQuery] string? keyword = null)
        {
            var q = _db.Tags.AsNoTracking();
            if (!string.IsNullOrWhiteSpace(keyword))
                q = q.Where(t => t.Name.Contains(keyword));

            var list = await q.OrderBy(t => t.Name)
                .Select(t => new TagDto
                {
                    TagId = t.TagID,
                    Name = t.Name,
                    Category = t.Category
                })
                .ToListAsync();


            return Ok(list);
        }
    }

}
