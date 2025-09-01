using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;

namespace Meow.Web.Controllers
{
    public class HomeController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Index()
        {
            // �I�s�ڭ̪��A��
            var data = await api.GetWeatherAsync();

            // ���ƶǵ� View
            return View(data);
        }
        public async Task<IActionResult> Tags()
        {
            var data = await api.GetTagsAsync();
            return View(data);
        }
    }
}