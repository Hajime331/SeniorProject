using System.Security.Claims;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Meow.Web.Models;
using Meow.Web.Services;

namespace Meow.Web.Controllers
{
    public class AuthController(IBackendApi api) : Controller
    {
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
                var me = await api.LoginAsync(vm.Email.Trim(), vm.Password);

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
                    return Redirect(vm.ReturnUrl);

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
    }
}
