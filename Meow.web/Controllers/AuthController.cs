using Meow.Web.Models;
using Meow.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Meow.Web.Controllers
{
    public partial class AuthController : Controller
    {
        private readonly IBackendApi _api;
        public AuthController(IBackendApi api) => _api = api;

        // GET: /Auth/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            return View(new LoginVm { ReturnUrl = returnUrl });
        }

        // POST: /Auth/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVm vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            try
            {
                var me = await _api.LoginAsync(vm.Email.Trim(), vm.Password);

                // 把 API 回來的使用者資訊變成 Claims
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, me!.MemberID.ToString()),
                    new Claim(ClaimTypes.Name, me.Nickname),
                    new Claim(ClaimTypes.Email, me.Email),
                    new Claim("IsAdmin", me.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(
                    claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                var authProps = new AuthenticationProperties
                {
                    IsPersistent = vm.RememberMe,           // 記住我
                    ExpiresUtc = DateTimeOffset.UtcNow.AddHours(8)
                };

                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    principal,
                    authProps);

                // 登入成功 → 回 ReturnUrl 或首頁
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                {
                    // 一般會員切掉 Admin 區的回跳，改導到前台首頁或我的儀表板
                    if (!me.IsAdmin && vm.ReturnUrl.StartsWith("/Admin", StringComparison.OrdinalIgnoreCase))
                    {
                        return RedirectToAction("MyWeekly", "Dashboard", new { area = "" });
                    }
                    return me.IsAdmin
                        ? RedirectToAction("Index", "Dashboard", new { area = "Admin" })
                        : RedirectToAction("MyWeekly", "Dashboard", new { area = "" });
                }

                TempData["Success"] = $"歡迎回來，{me.Nickname}";
                return RedirectToAction("Index", "Home");
            }
            catch (InvalidOperationException ex)
            {
                // Email 或密碼不正確（401）
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"無法連線到 API：{ex.Message}");
                return View(vm);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError(string.Empty, $"發生未預期錯誤：{ex.Message}");
                return View(vm);
            }
        }

        // POST: /Auth/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            TempData["Success"] = "你已登出";
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccessDenied()
        {
            Response.StatusCode = 403; // 保留正確狀態碼
            return View();
        }


        [HttpGet, AllowAnonymous]
        public IActionResult Register(string? returnUrl = null)
            => View(new RegisterVm { ReturnUrl = returnUrl });

        [HttpPost, ValidateAntiForgeryToken, AllowAnonymous]
        public async Task<IActionResult> Register(RegisterVm vm)
        {
            if (!ModelState.IsValid) return View(vm);

            try
            {
                // 呼叫 API：/api/Members 來建立會員
                var req = new MemberCreateRequest { Email = vm.Email.Trim(), Nickname = vm.Nickname.Trim(), Password = vm.Password };
                var created = await _api.CreateMemberAsync(req); // 若信箱重複會丟 InvalidOperationException
                // （選）自動登入：直接沿用你現有的登入邏輯
                var me = await _api.LoginAsync(vm.Email.Trim(), vm.Password);

                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, me!.MemberID.ToString()),
                    new Claim(ClaimTypes.Name, me.Nickname ?? me.Email),
                    new Claim(ClaimTypes.Email, me.Email),
                    new Claim("IsAdmin", me.IsAdmin.ToString())
                };

                var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                await HttpContext.SignInAsync(
                    CookieAuthenticationDefaults.AuthenticationScheme,
                    new ClaimsPrincipal(identity),
                    new AuthenticationProperties { IsPersistent = true, ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14) }
                );

                TempData["Success"] = "註冊成功，已自動為你登入。";
                if (!string.IsNullOrWhiteSpace(vm.ReturnUrl) && Url.IsLocalUrl(vm.ReturnUrl))
                    return Redirect(vm.ReturnUrl);

                // 一般會員導向你的個人儀表板（或首頁）
                return RedirectToAction("MyWeekly", "Dashboard");
            }
            catch (InvalidOperationException ex)
            {
                // 例如：Email 已被使用（API 回 409 時，BackendApi 會丟這個例外）
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(vm);
            }
            catch (HttpRequestException ex)
            {
                ModelState.AddModelError(string.Empty, $"無法連線到 API：{ex.Message}");
                return View(vm);
            }
        }
    }
}
