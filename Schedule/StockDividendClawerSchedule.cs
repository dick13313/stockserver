using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Repository;

namespace StockServer.Schedule
{
    public class StockDividendClawerSchedule : IInvocable
    {
        private readonly ILogger<StockDividendClawerSchedule> _logger;
        private StockDividendClawer _stockDividendClawer;
        private StockDividendRepository _stockDividendRepository;
        private StockInfoRepository _stockInfoRepository;
        public StockDividendClawerSchedule (
            ILogger<StockDividendClawerSchedule> logger,
            StockDividendClawer stockDividendClawer, 
            StockDividendRepository stockDividendRepository, 
            StockInfoRepository stockInfoRepository)
        {
            _logger = logger;
            _stockDividendClawer = stockDividendClawer;
            _stockDividendRepository = stockDividendRepository;
            _stockInfoRepository = stockInfoRepository;
        }

        public async Task Invoke()
        {
            _logger.LogInformation("StockDividendClawer Start");
            var stockIdList = _stockInfoRepository.GetAllStockIdByTypeAndIgnoreCategorys("上市", new List<string>() {
                "ETN",
                "指數投資證券(ETN)",
                "ETF",
                "上櫃指數股票型基金(ETF)",
                "存託憑證",
                "受益證券",
            });
            
            foreach (var stock_id in stockIdList)
            {
                try
                {
                    await _stockDividendClawer.ExecuteAsync(stock_id);
                    Thread.Sleep(6000);
                }
                catch (Exception ex)
                {
                    _logger.LogError("StockDividendClawer Error: \n" + ex.StackTrace);
                }
            }
        }
    }
}