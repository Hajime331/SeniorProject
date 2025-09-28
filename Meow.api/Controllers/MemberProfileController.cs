using Meow.Api.Data;
using Meow.Shared.Dtos.Accounts;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.IO;
using System.Collections.Generic;
using System.Linq;

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

    private static string GetAvatarUploadDirectory()
        => Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "uploads", "avatars");

    private void DeleteCustomAvatarFiles(Guid memberId)
    {
        var dir = GetAvatarUploadDirectory();
        if (!Directory.Exists(dir)) return;
        foreach (var path in Directory.EnumerateFiles(dir, $"{memberId}.*"))
        {
            try { System.IO.File.Delete(path); } catch { }
        }
    }

    private string? TryGetCustomAvatarUrl(Guid memberId)
    {
        var dir = GetAvatarUploadDirectory();
        if (!Directory.Exists(dir)) return null;
        var path = Directory.EnumerateFiles(dir, $"{memberId}.*").FirstOrDefault();
        if (path == null) return null;

        var fileName = Path.GetFileName(path);
        var version = System.IO.File.GetLastWriteTimeUtc(path).Ticks;
        return $"{Request.Scheme}://{Request.Host}/uploads/avatars/{fileName}?v={version}";
    }

    // GET /api/Members/{memberId}/profile
    [HttpGet("profile")]
    public async Task<ActionResult<MemberProfileDto>> GetProfile(Guid memberId)
    {
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

        profile ??= new MemberProfileDto { MemberID = memberId };

        if (string.IsNullOrWhiteSpace(profile.AvatarUrl))
        {
            var customUrl = TryGetCustomAvatarUrl(memberId);
            if (!string.IsNullOrWhiteSpace(customUrl))
            {
                profile.AvatarUrl = customUrl;
            }
        }

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
        var memberExists = await _db.Members.AsNoTracking()
            .AnyAsync(m => m.MemberID == memberId);
        if (!memberExists) return NotFound("Member not found.");

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

        DeleteCustomAvatarFiles(memberId);

        await _db.SaveChangesAsync();
        return NoContent();
    }

    [HttpPost("avatar/upload")]
    public async Task<IActionResult> UploadAvatar(Guid memberId, IFormFile file)
    {
        if (file == null || file.Length == 0) return BadRequest("No file.");

        var memberExists = await _db.Members.AsNoTracking()
            .AnyAsync(m => m.MemberID == memberId);
        if (!memberExists) return NotFound("Member not found.");

        var allowed = new Dictionary<string, string>
        {
            ["image/png"] = ".png",
            ["image/jpeg"] = ".jpg",
            ["image/webp"] = ".webp"
        };

        var contentType = file.ContentType?.ToLowerInvariant();
        if (!allowed.TryGetValue(contentType ?? string.Empty, out var ext))
        {
            ext = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (ext == ".jpeg") ext = ".jpg";
            if (ext != ".png" && ext != ".jpg" && ext != ".webp")
                return BadRequest("Unsupported image type.");
        }

        if (file.Length > 5 * 1024 * 1024) return BadRequest("Max 5MB.");

        var uploads = GetAvatarUploadDirectory();
        Directory.CreateDirectory(uploads);

        DeleteCustomAvatarFiles(memberId);

        var fileName = $"{memberId}{ext}";
        var destination = Path.Combine(uploads, fileName);
        using (var stream = new FileStream(destination, FileMode.Create, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream);
        }

        var now = DateTime.UtcNow;
        var profile = await _db.MemberProfiles.SingleOrDefaultAsync(p => p.MemberID == memberId);
        if (profile == null)
        {
            profile = new MemberProfile
            {
                MemberID = memberId,
                AvatarID = null,
                AvatarUpdatedAt = now
            };
            _db.MemberProfiles.Add(profile);
        }
        else
        {
            profile.AvatarID = null;
            profile.AvatarUpdatedAt = now;
        }

        await _db.SaveChangesAsync();

        var avatarUrl = $"{Request.Scheme}://{Request.Host}/uploads/avatars/{fileName}?v={now.Ticks}";
        return Ok(new { avatarUrl });
    }
}
