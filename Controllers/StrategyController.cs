using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using StockServer.Models.Query;
using StockServer.Strategy;
using StockServer.Repository;
using StockServer.Extensions;
using StockServer.Service;
using System;
using System.Linq;
using stockserver.Models.View;
using AutoMapper;

namespace StockServer.Controllers
{
    [ApiController]
    [Route("api/[controller]/[action]")]
    public class StrategyController : ControllerBase
    {
        private readonly ILogger<StrategyController> _logger;
        private StockRepository _stockRepository;
        private StockPickingService _stockPickingService;
        private IMapper _mapper;

        public StrategyController(
            ILogger<StrategyController> logger,
            StockRepository stockRepository,
            StockPickingService stockPickingService, IMapper mapper)
        {
            _logger = logger;
            _stockRepository = stockRepository;
            _stockPickingService = stockPickingService;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult Get0050KD([FromBody]StockQuery stockQuery)
        {
            var stockList = _stockRepository.GetStocks(stockQuery);
            var kdStrategy_buy = new KDStrategy(stockList).Calculate().K_SmallerThan(20);
            var kdStrategy_sell = new KDStrategy(stockList).Calculate().K_BiggerThan(80);
            return new JsonResult(new {
                all = new {
                    stockList = stockList,
                    array_k = kdStrategy_buy.outSlowK,
                    array_d = kdStrategy_buy.outSlowD,
                },
                buy = new {
                    stockList = stockList.UseStrategy(kdStrategy_buy),
                    array_k = kdStrategy_buy.GetResultKList(),
                    array_d = kdStrategy_buy.GetResultDList(),
                },
                sell = new {
                    stockList = stockList.UseStrategy(kdStrategy_sell),
                    array_k = kdStrategy_sell.GetResultKList(),
                    array_d = kdStrategy_sell.GetResultDList(),
                }
            });
        }

        [HttpPost]
        public IActionResult GetSeason_1(DateTime date)
        {
            var stockIds = _stockPickingService.GetPackingStockIds_1(date).ToList();
            if(stockIds != null && stockIds.Count > 0)
            {
                var rate = _stockPickingService.GetAverageRate(stockIds, date, date.AddYears(1));
                return new JsonResult(new { stockIds, rate });
            }
            else
                return new JsonResult(new { stockIds, Message = $"{date.ToString("YYYY/MM/dd")} 沒有符合條件的股票" });
        }

        [HttpGet]
        public IActionResult GetAverageIncrease(string stock_id, int averageDay, int totalDay)
        {
            var stockList = _stockRepository.GetSmallerThenDateStocks(stock_id, DateTime.Now.Date, totalDay);
            var stockCalculateModelList = stockList.Select((stock, index) => {
                var stockCalculateModel = _mapper.Map<StockCalculateModel>(stock);
                if(index >= 1)
                    stockCalculateModel.percent = Math.Round((Convert.ToDecimal(stock.close) / Convert.ToDecimal(stockList.ElementAt(index-1).close) - 1) * 100, 2, MidpointRounding.AwayFromZero);
                return stockCalculateModel;
            }).ToList();

            for(int index=0; index<stockCalculateModelList.Count(); index++)
            {
                decimal sum_percent_n = 0;
                
                if(index == averageDay-1)
                {
                    for(int i = 0 ; i < averageDay ; i++)
                    {
                        sum_percent_n += stockCalculateModelList.ElementAt(index - i).percent;
                    }
                    stockCalculateModelList.ElementAt(index).sum_percent_n = sum_percent_n;
                }
                if(index > averageDay-1)
                {
                    stockCalculateModelList.ElementAt(index).sum_percent_n = stockCalculateModelList.ElementAt(index-1).sum_percent_n + stockCalculateModelList.ElementAt(index).percent - stockCalculateModelList.ElementAt(index-averageDay).percent;
                }
                stockCalculateModelList.ElementAt(index).average_percent_n = Math.Round(stockCalculateModelList.ElementAt(index).sum_percent_n / averageDay, 2, MidpointRounding.AwayFromZero);
            }
            return new JsonResult(stockCalculateModelList);
        }
    }
}
