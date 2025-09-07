using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;
using Meow.Shared.Dtos;

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
    public async Task<IActionResult> Profile()
    {
        var id = User.GetUserId();
        var m = await _api.GetMemberAsync(id);
        var vm = new AccountProfileVm { Email = m.Email, Nickname = m.Nickname };
        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Profile(AccountProfileVm vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            var id = User.GetUserId();
            await _api.UpdateMemberNicknameAsync(id, vm.Nickname);
            TempData["Success"] = "暱稱已更新";
            return RedirectToAction(nameof(Profile));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "更新暱稱失敗");
            TempData["Error"] = "更新暱稱失敗，請稍後再試。";
            return View(vm);
        }
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
}
