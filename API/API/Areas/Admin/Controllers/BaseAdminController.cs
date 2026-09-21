using Dms.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = AppRoles.Admin)]
    public abstract class BaseAdminController : Controller
    {
    }
}
