using FastPMS.Models;
using FastPMS.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;
using FastPMS.Models;
using FastPMS.Services;

namespace YourProjectName.Controllers
{
    [Authorize(Roles = "SuperAdmin,Admin,TeamMember")]
    public class AIChatController : Controller
    {
        private readonly IDeepSeekService _deepSeekService;

        public AIChatController(IDeepSeekService deepSeekService)
        {
            _deepSeekService = deepSeekService;
        }

        [HttpPost]
        public async Task<JsonResult> SendMessage([FromBody] AIChatModel model)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(model.Message))
                {
                    return Json(new { success = false, error = "Message cannot be empty" });
                }

                var aiResponse = await _deepSeekService.GetAIResponseAsync(model.Message);

                var responseModel = new AIChatModel
                {
                    Message = model.Message,
                    Response = aiResponse,
                    IsFromUser = false,
                    Timestamp = DateTime.Now
                };

                return Json(new { success = true, data = responseModel });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, error = ex.Message });
            }
        }

        [HttpGet]
        public async Task<JsonResult> CheckApiStatus()
        {
            var isActive = await _deepSeekService.IsApiKeyValidAsync();
            return Json(new { active = isActive });
        }
    }
}