using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockServer.Repository;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class RevenueController
    {
        private readonly ILogger<RevenueController> _logger;
        private RevenueRepository _revenueRepository;

        public RevenueController(ILogger<RevenueController> logger, RevenueRepository revenueRepository)
        {
            _logger = logger;
            _revenueRepository = revenueRepository;
        }

        [HttpGet]
        public IActionResult GetMonthRevenueIncrease(int year, int month)
        {
            return new JsonResult(_revenueRepository.GetMonthRevenueIncrease(year, month));
        }

        [HttpGet]
        public IActionResult GetSeasonRevenueIncrease(int year, int season)
        {
            return new JsonResult(_revenueRepository.GetSeasonRevenueIncrease(year, season));
        }
        
    }
}