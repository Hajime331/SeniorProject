using Meow.Api.Data;
using Meow.Shared.Dtos.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace Meow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → 路徑會是 /api/Members
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _db; // ← 你的 DbContext 名稱

        public MembersController(AppDbContext db)
        {
            _db = db;
        }

        // 查全部會員（不含密碼雜湊）
        [HttpGet] // GET /api/Members
        public async Task<ActionResult<IEnumerable<MemberListDto>>> GetAll()
        {
            var data = await _db.Members
                .AsNoTracking() // 只讀，省資源，此刻還不修改資料
                .Select(m => new MemberListDto  // 挑選以 DTO 形狀輸出，避免外洩欄位
                {
                    MemberID = m.MemberID,
                    Email = m.Email,
                    Nickname = m.Nickname,
                    CreatedAt = m.CreatedAt,
                    Status = m.Status,
                    IsAdmin = m.IsAdmin,
                    LastLoginAt = m.LastLoginAt
                })
                .ToListAsync();

            return Ok(data); // 200 + JSON
        }

        // 查單一會員（不含密碼雜湊）
        // {id:guid} 代表路由參數必須是 Guid 格式
        [HttpGet("{id:guid}")]
        public async Task<ActionResult<MemberListDto>> GetOne(Guid id)
        {
            var m = await _db.Members
                .AsNoTracking()
                .Where(x => x.MemberID == id)
                .Select(x => new MemberListDto
                {
                    MemberID = x.MemberID,
                    Email = x.Email,
                    Nickname = x.Nickname,
                    CreatedAt = x.CreatedAt,
                    Status = x.Status,
                    IsAdmin = x.IsAdmin,
                    LastLoginAt = x.LastLoginAt
                })
                .FirstOrDefaultAsync();

            return m is null ? NotFound() : Ok(m);
        }

        // 註冊新會員
        [HttpPost]
        [ProducesResponseType(typeof(MemberListDto), StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status409Conflict)]

        // 明確宣告 body 來的 JSON
        public async Task<IActionResult> Create([FromBody] MemberCreateDto input) 
        {
            // 1. 標準化 Email：去頭尾空白、統一小寫，用來檢查重複
            var normalized = input.Email.Trim().ToLowerInvariant();

            // 2. 查是否已存在（只讀查詢用 AsNoTracking）
            var exists = await _db.Members
                .AsNoTracking()
                .AnyAsync(m => m.EmailNormalized == normalized);
            if (exists)
            {
                // 2a. 有衝突就回 409，這比回 400 更精準（表示資源狀態衝突）
                return Conflict("Email 已被使用");
            }

            // 3. 產生密碼雜湊（BCrypt 內含隨機鹽值，安全性足夠）
            var hash = BCrypt.Net.BCrypt.HashPassword(input.Password);

            // 4. 建立 Entity 並設定初值（UTC 時間、狀態、管理者預設 false）
            // 注意：EmailNormalized 是計算欄位，不用手動賦值
            var member = new Member
            {
                MemberID = Guid.NewGuid(),
                Email = input.Email.Trim(),
                PasswordHash = hash,
                Nickname = input.Nickname.Trim(),
                CreatedAt = DateTime.UtcNow,
                IsAdmin = false,
                Status = "Active",
                LastLoginAt = null
            };

            // 5. 寫入資料庫
            _db.Members.Add(member);
            await _db.SaveChangesAsync();

            // 6. 組回傳 DTO（不回密碼欄位）
            var dto = new MemberListDto
            {
                MemberID = member.MemberID,
                Email = member.Email,
                Nickname = member.Nickname,
                CreatedAt = member.CreatedAt,
                Status = member.Status,
                IsAdmin = member.IsAdmin,
                LastLoginAt = member.LastLoginAt
            };

            // 7. 回傳 201 建立成功，並帶上 Location 標頭
            return CreatedAtAction(nameof(GetOne), new { id = member.MemberID }, dto);
        }

        [HttpPost("login")]
        [Consumes("application/json")]
        [Produces("application/json")]
        [ProducesResponseType(typeof(MemberListDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Login([FromBody] MemberLoginDto input)
        {
            var normalized = input.Email.Trim().ToLowerInvariant();

            var m = await _db.Members
                .FirstOrDefaultAsync(x => x.EmailNormalized == normalized);

            // 不要暴露「帳號不存在」細節，統一回 401
            if (m is null || !BCrypt.Net.BCrypt.Verify(input.Password, m.PasswordHash))
                return Unauthorized(new { message = "Email 或密碼不正確" });

            // （可選）更新最後登入時間
            m.LastLoginAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            // 回傳給前端可用的安全欄位
            var dto = new MemberListDto
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

        [HttpGet("count")]
        public async Task<int> Count()
        {
            return await _db.Members.AsNoTracking().CountAsync();
        }

        [HttpGet("recent")]
        public async Task<IEnumerable<MemberListDto>> Recent([FromQuery] int take = 5)
        {
            return await _db.Members.AsNoTracking()
                .OrderByDescending(m => m.CreatedAt)
                .Take(take)
                .Select(m => new MemberListDto
                {
                    MemberID = m.MemberID,
                    Email = m.Email,
                    Nickname = m.Nickname,
                    IsAdmin = m.IsAdmin,
                    CreatedAt = m.CreatedAt
                })
                .ToListAsync();
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] MemberUpdateNicknameDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Nickname) || dto.Nickname.Length > 80)
                return ValidationProblem("暱稱必填且長度需 ≤ 80。");

            var member = await _db.Members.SingleOrDefaultAsync(m => m.MemberID == id);
            if (member == null) return NotFound();

            // 身分檢查先拿掉，避免 Web 呼叫 API 沒帶 Token 時卡住
            // var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            // var isAdmin = User.HasClaim("IsAdmin", "True");
            // if (!isAdmin && (!Guid.TryParse(userIdStr, out var uid) || uid != id))
            //     return Forbid();

            member.Nickname = dto.Nickname.Trim();
            await _db.SaveChangesAsync();

            return NoContent(); // 204
        }


        [HttpPut("{id:guid}/password")]
        public async Task<IActionResult> ChangePassword(Guid id, [FromBody] ChangePasswordDto dto)
        {
            if (!ModelState.IsValid) return ValidationProblem(ModelState);

            var member = await _db.Members.SingleOrDefaultAsync(m => m.MemberID == id);
            if (member == null) return NotFound();

            // 驗證目前密碼
            if (!BCrypt.Net.BCrypt.Verify(dto.CurrentPassword, member.PasswordHash))
                return ValidationProblem("目前密碼不正確。");

            // 可加自訂密碼規則（例如含數字大小寫等）
            if (dto.NewPassword == dto.CurrentPassword)
                return ValidationProblem("新密碼不可與目前密碼相同。");

            // 變更密碼
            member.PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.NewPassword);
            await _db.SaveChangesAsync();
            return NoContent(); // 204
        }
    }
}