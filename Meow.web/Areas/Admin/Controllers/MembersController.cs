using Meow.Web.Models;
using Meow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Meow.Web.ViewModels;

namespace Meow.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Policy = "AdminOnly")] // 你已經有 AdminOnly Policy
    public class MembersController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var data = await api.GetMembersAsync();
            return View(data);
        }

        [HttpGet]
        public IActionResult Create() => View(new MemberCreateVm());

        // POST: /Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberCreateVm vm)
        {
            if (!ModelState.IsValid) return View(vm);
            try
            {
                var req = new MemberCreateRequest { Email = vm.Email.Trim(), Nickname = vm.Nickname.Trim(), Password = vm.Password };
                var created = await api.CreateMemberAsync(req);
                TempData["Success"] = $"已建立會員：{created?.Nickname}";
                return RedirectToAction(nameof(Index));
            }
            catch (InvalidOperationException ex)
            {
                // 409 / Email 已被使用 → 顯示在表單上方
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch (HttpRequestException ex)
            {
                // 網路/位址錯誤等
                ModelState.AddModelError(string.Empty, $"無法連線到 API：{ex.Message}");
                return View(vm);
            }
            catch (Exception ex)
            {
                // 其他未預期錯誤
                ModelState.AddModelError(string.Empty, $"發生未預期錯誤：{ex.Message}");
                return View(vm);
            }
        }
    }
}
