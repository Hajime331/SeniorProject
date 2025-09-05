using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;

[Authorize]
public class AccountController : Controller
{
    private readonly IBackendApi _api;
    public AccountController(IBackendApi api) => _api = api;

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
            TempData["Error"] = "更新暱稱失敗，請稍後再試。";
            // 可記 log：ex
            return View(vm);
        }
    }
}
