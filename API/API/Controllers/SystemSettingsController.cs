using Dms.Application.Common;
using Dms.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers
{
    public class SystemSettingsController : BaseApiController
    {
        [HttpGet]
        [Authorize(Policy = Permissions.SystemSettings.View)]
        public IActionResult GetSettings()
        {
            return Ok(new { Message = "Access granted to system settings with permission!" });
        }
    }
}
