using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;

namespace Meow.Web.Controllers
{
    public class HomeController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Weather()
        {
            // 呼叫我們的服務
            var data = await api.GetWeatherAsync();

            // 把資料傳給 View
            return View(data);
        }
    }
}