using Microsoft.AspNetCore.Mvc;
using Meow.Web.Services;

namespace Meow.Web.Controllers
{
    public class HomeController(IBackendApi api) : Controller
    {
        public async Task<IActionResult> Weather()
        {
            // �I�s�ڭ̪��A��
            var data = await api.GetWeatherAsync();

            // ���ƶǵ� View
            return View(data);
        }
    }
}