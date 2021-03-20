using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Slack.Webhooks;
using StockServer.Service;
using SlackClient = Slack.Webhooks.SlackClient;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class TestController : ControllerBase
    {
        private readonly ILogger<TestController> _logger;
        private SlackClient _slackClient;
        private StockPickingService _stockPickingService;
        public TestController(
            ILogger<TestController> logger, 
            SlackClient slackClient, 
            StockPickingService stockPickingService)
        {
            _logger = logger;
            _slackClient = slackClient;
            _stockPickingService = stockPickingService;
        }

        [HttpPost]
        public IActionResult SendMessageToSlack(string message)
        {
            var slackTask = _slackClient.PostAsync(new SlackMessage() {
                // Text = message,
                Blocks = new List<Block>(){
                    new Slack.Webhooks.Blocks.Image
                    {
                        AltText = "Sexy Skyline",
                        ImageUrl = "https://placekitten.com/500/500",
                        Title = new Slack.Webhooks.Elements.TextObject("Hello, this is text")
                    }
                } 
            });
            return new JsonResult(new { Success = true });
        }

        [HttpGet]
        public IActionResult TestSlackLog(string message)
        {
            _logger.LogDebug(message);
            return new JsonResult(new
            {
                Success = true
            });
        }

        [HttpGet]
        public IActionResult Test()
        {
            return new JsonResult(new
            {
                Success = true
            });
        }
        
    }
}