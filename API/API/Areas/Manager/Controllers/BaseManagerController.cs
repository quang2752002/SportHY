using Dms.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Admin + "," + AppRoles.Manager)]
    public abstract class BaseManagerController : Controller
    {
    }
}
