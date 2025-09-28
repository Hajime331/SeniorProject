using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;
using Meow.Shared.Dtos;
using Meow.Shared.Dtos.Accounts;

[Authorize]
public class AccountController : Controller
{
    private readonly IBackendApi _api;
    private readonly ILogger<AccountController> _logger;

    public AccountController(IBackendApi api, ILogger<AccountController> logger)
    {
        _api = api;
        _logger = logger;
    }

    [HttpGet]
    public IActionResult ChangePassword() => View(new ChangePasswordVm());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var id = User.GetUserId();
            var dto = new ChangePasswordDto
            {
                CurrentPassword = vm.CurrentPassword,
                NewPassword = vm.NewPassword,
                ConfirmNewPassword = vm.ConfirmNewPassword
            };
            await _api.ChangePasswordAsync(id, dto);
            TempData["Success"] = "密碼已更新。";
            return RedirectToAction(nameof(ChangePassword));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "變更密碼失敗");
            TempData["Error"] = "變更密碼時發生錯誤，請確認目前密碼並稍後重試。";
            return View(vm);
        }
    }

    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var id = User.GetUserId();
        var member = await _api.GetMemberAsync(id);
        var profile = await _api.GetMemberProfileAsync(id);
        var avatars = await _api.GetAvatarsAsync();

        var vm = new AccountProfileVm
        {
            Email = member.Email,
            Nickname = member.Nickname,
            BirthDate = profile?.BirthDate,
            Gender = profile?.Gender,
            HeightCm = profile?.HeightCm,
            WeightKg = profile?.WeightKg,
            AvatarID = profile?.AvatarID,
            AvatarUrl = profile?.AvatarUrl,
            Avatars = avatars
        };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(AccountProfileVm vm)
    {
        if (!ModelState.IsValid)
        {
            vm.Avatars = await _api.GetAvatarsAsync();
            return View(vm);
        }

        try
        {
            var id = User.GetUserId();
            await _api.UpdateMemberNicknameAsync(id, vm.Nickname);

            var dto = new MemberProfileUpdateDto
            {
                BirthDate = vm.BirthDate,
                Gender = vm.Gender,
                HeightCm = vm.HeightCm,
                WeightKg = vm.WeightKg
            };
            await _api.UpdateMemberProfileAsync(id, dto);

            if (vm.AvatarFile != null && vm.AvatarFile.Length > 0)
            {
                var avatarUrl = await _api.UploadMemberAvatarAsync(id, vm.AvatarFile);
                if (string.IsNullOrWhiteSpace(avatarUrl))
                {
                    TempData["Error"] = "頭貼上傳失敗，請稍後再試。";
                }
            }
            else if (vm.AvatarID.HasValue)
            {
                await _api.UpdateMemberAvatarAsync(id, vm.AvatarID.Value);
            }

            TempData["Success"] = "個人資料已更新。";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新個人資料失敗");
            TempData["Error"] = "更新個人資料時發生問題，請稍後再試。";
            vm.Avatars = await _api.GetAvatarsAsync();
            return View(vm);
        }
    }
}
