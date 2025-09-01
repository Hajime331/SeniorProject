using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Meow.Api.Dtos;
using Meow.Api.Data;

namespace Meow.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")] // → 路徑會是 /api/Members
    public class MembersController : ControllerBase
    {
        private readonly AppDbContext _db; // ← 你的 DbContext 名稱

        public MembersController(AppDbContext db) => _db = db;

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
            var emailLower = input.Email.Trim().ToLowerInvariant();

            // 2. 查是否已存在（只讀查詢用 AsNoTracking）
            var exists = await _db.Members
                .AsNoTracking()
                .AnyAsync(m => m.EmailNormalized == emailLower);
            if (exists)
            {
                // 2a. 有衝突就回 409，這比回 400 更精準（表示資源狀態衝突）
                return Conflict(new { message = "Email 已被使用" });
            }

            // 3. 產生密碼雜湊（BCrypt 內含隨機鹽值，安全性足夠）
            var hash = BCrypt.Net.BCrypt.HashPassword(input.Password);

            // 4. 建立 Entity 並設定初值（UTC 時間、狀態、管理者預設 false）
            // 注意：EmailNormalized 是計算欄位，不用手動賦值
            var entity = new Member
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
            _db.Members.Add(entity);
            await _db.SaveChangesAsync();

            // 6. 組回傳 DTO（不回密碼欄位）
            var dto = new MemberListDto
            {
                MemberID = entity.MemberID,
                Email = entity.Email,
                Nickname = entity.Nickname,
                CreatedAt = entity.CreatedAt,
                Status = entity.Status,
                IsAdmin = entity.IsAdmin,
                LastLoginAt = entity.LastLoginAt
            };

            // 7. 回傳 201 建立成功，並帶上 Location 標頭
            return CreatedAtAction(nameof(GetOne), new { id = entity.MemberID }, dto);
        }

    }
}