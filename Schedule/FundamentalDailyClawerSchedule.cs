using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Repository;

namespace StockServer.Schedule
{
    public class FundamentalDailyClawerSchedule : IInvocable
    {
        private readonly ILogger<FundamentalDailyClawerSchedule> _logger;
        private FundamentalDailyClawer _fundamentalDailyClawer;
        private FundamentalDailyRepository _fundamentalDailyRepository;
        public FundamentalDailyClawerSchedule(ILogger<FundamentalDailyClawerSchedule> logger, FundamentalDailyClawer fundamentalDailyClawer, FundamentalDailyRepository fundamentalDailyRepository)
        {
            _logger = logger;
            _fundamentalDailyClawer = fundamentalDailyClawer;
            _fundamentalDailyRepository = fundamentalDailyRepository;
        }
        public async Task Invoke()
        {
            _logger.LogInformation($"FundamentalDailyClawerSchedule Start");

            for(var date = DateTime.Now; DateTime.Compare(date, new DateTime(2015, 1, 1)) == 1; date = date.AddDays(-1))
            {
                if(date.DayOfWeek == DayOfWeek.Sunday || date.DayOfWeek == DayOfWeek.Saturday) 
                    continue;
                if(!_fundamentalDailyRepository.IsExist(date))
                {
                    await _fundamentalDailyClawer.ExecuteAsync(date);
                    Thread.Sleep(6000);
                }
            }
        }
    }
}