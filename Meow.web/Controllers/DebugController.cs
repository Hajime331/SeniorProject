using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Meow.Web.Controllers
{
    [AllowAnonymous]
    public class DebugController : Controller
    {
        [HttpGet("/debug/whereami")]
        public IActionResult WhereAmI()
        {
            var area = (string?)RouteData.Values["area"] ?? "(none)";
            var ctrl = (string?)RouteData.Values["controller"] ?? "(unknown)";
            var act = (string?)RouteData.Values["action"] ?? "(unknown)";
            var isAdmin = User?.Claims?.Any(c => c.Type == "IsAdmin" && c.Value == "True") == true;

            return Content(
$@"Area: {area}
Controller: {ctrl}
Action: {act}
IsAdmin: {isAdmin}
User: {User?.Identity?.Name ?? "(anon)"}",
                "text/plain");
        }
    }
}
