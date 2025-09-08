using Meow.Web.Models;
using Meow.Web.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Meow.Web.ViewModels;

namespace Meow.Web.Controllers
{
    [Authorize]  // 只要有登入就能使用
    // 只做「叫服務、把資料丟給 View」
    public class MembersController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var data = await api.GetMembersAsync();
            return View(data);
        }

        // GET: /Members/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View(new MemberCreateVm());
        }

        // POST: /Members/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(MemberCreateVm vm)
        {
            // 1) 伺服器端驗證（對應 DataAnnotations），不合格（例如 Email 空白）→ 這行會原頁返回
            if (!ModelState.IsValid)
            {
                return View(vm);
            }

            try
            {
                // 2) 呼叫後端 API
                var req = new MemberCreateRequest
                {
                    Email = vm.Email.Trim(),
                    Nickname = vm.Nickname.Trim(),
                    Password = vm.Password
                };

                var created = await api.CreateMemberAsync(req);

                // 3) 成功 → 設置提示訊息並回清單
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
