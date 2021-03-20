using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Repository;

namespace StockServer.Schedule
{
    public class StockHolderClawerSchedule : IInvocable
    {
        private StockHolderClawer _stockHolderClawer;
        private StockHolderRepository _stockHolderRepository;
        private StockInfoRepository _stockInfoRepository;
        private readonly ILogger<StockHolderClawerSchedule> _logger;

        public StockHolderClawerSchedule (ILogger<StockHolderClawerSchedule> logger, StockHolderClawer stockHolderClawer, StockHolderRepository stockHolderRepository, StockInfoRepository stockInfoRepository)
        {
            _stockHolderRepository = stockHolderRepository;
            _stockInfoRepository = stockInfoRepository;
            _stockHolderClawer = stockHolderClawer;
            _logger = logger;
        }
        public async Task Invoke()
        {
            _logger.LogInformation("StockHolderClawerSchedule Start");

            var stockIdList = _stockInfoRepository.GetAllStockIdByType("上市");   
            var dateStringList = (await _stockHolderClawer.GetDateStringListByTDCCAsync()).ToList();  

            foreach(var dateString in dateStringList)
            {
                if(dateString == dateStringList.ElementAt(0))
                    await _stockHolderClawer.ExecuteAsync();
                else 
                {
                    foreach(var stockId in stockIdList)
                    {
                        if(!_stockHolderRepository.IsExist(stockId, dateString))
                        {
                            _logger.LogInformation($"StockHolderClawer Execute: {stockId}, {dateString}");
                            Task clawerTask = _stockHolderClawer.ExecuteAsync(stockId, dateString);
                            await Task.WhenAll (clawerTask, Task.Run(() => Thread.Sleep(6000)));
                        }
                    }
                }
                
            }      
        }
    }
}