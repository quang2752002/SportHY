using Dms.Application.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Areas.Manager.Controllers
{
    [Area("Manager")]
    [Authorize(Roles = AppRoles.Manager + "," + AppRoles.Admin)]
    public abstract class BaseManagerController : Controller
    {
    }
}
