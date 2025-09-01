using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;

namespace Meow.Web.Controllers
{
    // 只做「叫服務、把資料丟給 View」
    public class MembersController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Index()
        {
            var data = await api.GetMembersAsync();
            return View(data);
        }
    }
}
