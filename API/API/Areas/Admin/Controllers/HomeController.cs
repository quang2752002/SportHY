using Microsoft.AspNetCore.Mvc;

namespace API.Areas.Admin.Controllers
{
    public class HomeController : BaseAdminController
    {
        public IActionResult Index()
        {
            // Điều hướng mặc định đến Quản lý Giải đấu
            return RedirectToAction("Index", "GiaiDau");
        }
    }
}
