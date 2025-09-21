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


        [HttpGet("{id:guid}")]
        public async Task<ActionResult<TagDto>> GetById(Guid id)
        {
            var t = await _db.Tags.AsNoTracking().FirstOrDefaultAsync(x => x.TagID == id);
            if (t is null) return NotFound();
            return Ok(new TagDto { TagId = t.TagID, Name = t.Name, Category = t.Category });
        }

        [HttpPost]
        public async Task<ActionResult<TagDto>> Create([FromBody] TagCreateDto dto)
        {
            if (dto is null) return BadRequest("payload required");
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required");
            if (string.IsNullOrWhiteSpace(dto.Category)) return BadRequest("Category required");

            // 簡單驗證：名稱唯一（你的 DB 對 Name 有 UNIQUE）
            var exists = await _db.Tags.AnyAsync(x => x.Name == dto.Name);
            if (exists) return Conflict("Tag name already exists.");

            var e = new Meow.Api.Data.Tag
            {
                TagID = Guid.NewGuid(),
                Name = dto.Name.Trim(),
                Category = dto.Category.Trim()
            };
            _db.Tags.Add(e);
            await _db.SaveChangesAsync();

            var result = new TagDto { TagId = e.TagID, Name = e.Name, Category = e.Category };
            return CreatedAtAction(nameof(GetById), new { id = e.TagID }, result);
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] TagUpdateDto dto)
        {
            if (dto is null) return BadRequest("payload required");
            if (string.IsNullOrWhiteSpace(dto.Name)) return BadRequest("Name required");
            if (string.IsNullOrWhiteSpace(dto.Category)) return BadRequest("Category required");

            var e = await _db.Tags.FirstOrDefaultAsync(x => x.TagID == id);
            if (e is null) return NotFound();

            // 若更名，需要檢查唯一性
            var nameChanged = !string.Equals(e.Name, dto.Name, StringComparison.Ordinal);
            if (nameChanged)
            {
                var exists = await _db.Tags.AnyAsync(x => x.Name == dto.Name && x.TagID != id);
                if (exists) return Conflict("Tag name already exists.");
            }

            e.Name = dto.Name.Trim();
            e.Category = dto.Category.Trim();
            await _db.SaveChangesAsync();

            return NoContent();
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var e = await _db.Tags.FirstOrDefaultAsync(x => x.TagID == id);
            if (e is null) return NotFound();

            _db.Tags.Remove(e); // 你對 VideoTagMap / SetTagMap 是 FK ON DELETE CASCADE，會自動清掉關聯
            await _db.SaveChangesAsync();

            return NoContent();
        }
    }

}
