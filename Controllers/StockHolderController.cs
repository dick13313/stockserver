using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockServer.Repository;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StockHolderController : ControllerBase
    {
        private readonly ILogger<StockHolderController> _logger;
        private StockHolderRepository _stockHolderRepository;
        public StockHolderController(ILogger<StockHolderController> logger, StockHolderRepository stockHolderRepository)
        {
            _logger = logger;
            _stockHolderRepository = stockHolderRepository;
        }

        [HttpGet]
        public IActionResult GetIncrease(string dateString1, string dateString2)
        {
            return new JsonResult(_stockHolderRepository.GetIncrease(dateString1, dateString2));
        }
    }
}