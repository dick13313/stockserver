using System;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using StockServer.Models.Query;
using StockServer.Repository;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StockController : ControllerBase
    {
        private readonly ILogger<StockController> _logger;
        private StockApiRepository _stockApiRepository;
        private StockInfoRepository _stockInfoRepository;
        private StockRepository _stockRepository;

        public StockController(ILogger<StockController> logger, StockRepository stockRepository, StockInfoRepository stockInfoRepository, StockApiRepository stockApiRepository)
        {
            _logger = logger;
            _stockRepository = stockRepository;
            _stockInfoRepository = stockInfoRepository;
            _stockApiRepository = stockApiRepository;
        }

        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public IActionResult GetStockInfo()
        {
            return new JsonResult(_stockInfoRepository.GetAll());
        }

        [HttpGet]
        public IActionResult GetStockInfoByStockId(string stock_id)
        {
            return new JsonResult(_stockInfoRepository.GetStockInfo(stock_id));
        }

        [HttpGet]
        [ResponseCache(Duration = 3600)]
        public IActionResult GetStockCategorys()
        {
            return new JsonResult(_stockInfoRepository.StockCategorys());
        }

        [HttpPost]
        public IActionResult GetStocks([FromBody]StockQuery stockQuery)
        {
            try
            {
                return new JsonResult(_stockRepository.GetStocks(stockQuery));
            }
            catch (Exception ex)
            {
                return new JsonResult(ex.Message);
            }
        }
    }
}
