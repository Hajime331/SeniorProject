using Meow.Api.Data;
using Meow.Shared.Dtos.Accounts;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Meow.Api.Controllers;

[ApiController]
[Route("api/Members/{memberId:guid}")]
public class MemberProfileController : ControllerBase
{
    private readonly AppDbContext _db;

    public MemberProfileController(AppDbContext db)
    {
        _db = db;
    }

    // GET /api/Members/{memberId}/profile
    [HttpGet("profile")]
    public async Task<ActionResult<MemberProfileDto>> GetProfile(Guid memberId)
    {
        // 確認會員存在
        var exists = await _db.Members.AsNoTracking().AnyAsync(m => m.MemberID == memberId);
        if (!exists) return NotFound("Member not found.");

        var profile = await _db.MemberProfiles
            .AsNoTracking()
            .Where(p => p.MemberID == memberId)
            .Select(p => new MemberProfileDto
            {
                MemberID = p.MemberID,
                BirthDate = p.BirthDate,
                Gender = p.Gender,
                AvatarID = p.AvatarID,
                AvatarUrl = p.Avatar != null ? p.Avatar.ImageUrl : null,
                HeightCm = p.HeightCm,
                WeightKg = p.WeightKg
            })
            .FirstOrDefaultAsync();

        // 若尚未建立資料，回傳空白 DTO
        profile ??= new MemberProfileDto { MemberID = memberId };
        return Ok(profile);
    }

    // PUT /api/Members/{memberId}/profile
    [HttpPut("profile")]
    public async Task<IActionResult> UpdateProfile(Guid memberId, [FromBody] MemberProfileUpdateDto dto)
    {
        if (!ModelState.IsValid) return ValidationProblem(ModelState);

        var member = await _db.Members.FindAsync(memberId);
        if (member == null) return NotFound("Member not found.");

        var profile = await _db.MemberProfiles.SingleOrDefaultAsync(p => p.MemberID == memberId);
        if (profile == null)
        {
            profile = new MemberProfile
            {
                MemberID = memberId,
                AvatarUpdatedAt = DateTime.UtcNow
            };
            _db.MemberProfiles.Add(profile);
        }

        profile.BirthDate = dto.BirthDate;
        profile.Gender = string.IsNullOrWhiteSpace(dto.Gender) ? null : dto.Gender!.Trim();
        profile.HeightCm = dto.HeightCm;
        profile.WeightKg = dto.WeightKg;

        await _db.SaveChangesAsync();
        return NoContent();
    }

    // PUT /api/Members/{memberId}/avatar/{avatarId}
    [HttpPut("avatar/{avatarId:guid}")]
    public async Task<IActionResult> UpdateAvatar(Guid memberId, Guid avatarId)
    {
        // 驗證會員是否存在
        var memberExists = await _db.Members.AsNoTracking()
            .AnyAsync(m => m.MemberID == memberId);
        if (!memberExists) return NotFound("Member not found.");

        // 驗證頭像存在且為啟用狀態
        var avatar = await _db.AvatarCatalogs.AsNoTracking()
            .FirstOrDefaultAsync(a => a.AvatarID == avatarId && a.Status == "Active");
        if (avatar == null) return NotFound("Avatar not found.");

        var profile = await _db.MemberProfiles.SingleOrDefaultAsync(p => p.MemberID == memberId);
        if (profile == null)
        {
            profile = new MemberProfile
            {
                MemberID = memberId,
                AvatarID = avatarId,
                AvatarUpdatedAt = DateTime.UtcNow
            };
            _db.MemberProfiles.Add(profile);
        }
        else
        {
            profile.AvatarID = avatarId;
            profile.AvatarUpdatedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        return NoContent();
    }
}