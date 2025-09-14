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
            var id = User.GetUserId(); // 你已實作的擴充方法（TryParse + 丟例外版）
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
            TempData["Error"] = "變更密碼失敗，請確認目前密碼或稍後再試。";
            return View(vm);
        }
    }


    [HttpGet]
    public async Task<IActionResult> Profile()
    {
        var id = User.GetUserId();
        // 取得基本會員資料與擴展資料
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
            // 若驗證失敗，重新載入可選頭像列表以供顯示
            vm.Avatars = await _api.GetAvatarsAsync();
            return View(vm);
        }

        try
        {
            var id = User.GetUserId();
            // 更新暱稱
            await _api.UpdateMemberNicknameAsync(id, vm.Nickname);

            // 更新擴展資料
            var dto = new MemberProfileUpdateDto
            {
                BirthDate = vm.BirthDate,
                Gender = vm.Gender,
                HeightCm = vm.HeightCm,
                WeightKg = vm.WeightKg
            };
            await _api.UpdateMemberProfileAsync(id, dto);

            // 若有選擇頭像，呼叫 API 更新頭像
            if (vm.AvatarID.HasValue)
                await _api.UpdateMemberAvatarAsync(id, vm.AvatarID.Value);

            TempData["Success"] = "個人資料已更新。";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新個人資料失敗");
            TempData["Error"] = "更新個人資料失敗，請稍後再試。";
            // 失敗時仍需重新載入頭像清單
            vm.Avatars = await _api.GetAvatarsAsync();
            return View(vm);
        }
    }
}
