using AIComunicateAPI.Models;
using AIComunicateAPI.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIComunicateAPI.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class AIComunicateController : ControllerBase
    {
        private readonly ILogger<AIComunicateController> _logger;
        private readonly AIService aIService;

        public AIComunicateController(ILogger<AIComunicateController> logger, AIService aIService)
        {
            _logger = logger;
            this.aIService = aIService;
        }

        [HttpPost]
        public async Task<ActionResult> GenerateText(ReceiveDto receiveDto)
        {
            var response = await this.aIService.GenerateTextAsync(receiveDto.Message);
            return this.Ok(response);
        }
    }
}
