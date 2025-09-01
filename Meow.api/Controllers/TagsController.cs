using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;  // Scaffold 產生的 DbContext 和 Entity 類別所在命名空間

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

        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var tags = await _db.Tags.AsNoTracking().ToListAsync();
            return Ok(tags);
        }
    }
}
