using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Data;
using Meow.Shared.Dtos.Accounts;

namespace Meow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → /api/Auth
    public class AuthController : ControllerBase
    {
        private readonly AppDbContext _db;
        public AuthController(AppDbContext db) { _db = db; }

        [HttpPost("login")]
        [ProducesResponseType(typeof(MemberListItemDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] MemberLoginDto input)
        {
            var normalized = input.Email.Trim().ToLowerInvariant();
            var m = await _db.Members.FirstOrDefaultAsync(x => x.EmailNormalized == normalized);
            if (m is null || !BCrypt.Net.BCrypt.Verify(input.Password, m.PasswordHash))
                return Unauthorized(new { message = "Email 或密碼不正確" });

            m.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 只回 DTO，不建立 Cookie
            var dto = new MemberListItemDto
            {
                MemberID = m.MemberID,
                Email = m.Email,
                Nickname = m.Nickname,
                CreatedAt = m.CreatedAt,
                Status = m.Status,
                IsAdmin = m.IsAdmin,
                LastLoginAt = m.LastLoginAt
            };
            return Ok(dto);
        }
    }
}
