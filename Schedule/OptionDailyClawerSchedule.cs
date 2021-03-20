using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Const;
using StockServer.Models.Query;

namespace StockServer.Schedule
{
    public class OptionDailyClawerSchedule : IInvocable
    {
        private readonly ILogger<OptionDailyClawerSchedule> _logger;
        private OptionDailyClawer _optionDailyClawer;
        public OptionDailyClawerSchedule(ILogger<OptionDailyClawerSchedule> logger, OptionDailyClawer optionDailyClawer)
        {
            _logger = logger;
            _optionDailyClawer = optionDailyClawer;
        }
        public async Task Invoke()
        {
            _logger.LogInformation($"OptionDailyClawerSchedule Start");
            await _optionDailyClawer.ExecuteAsync(new OptionDailyQuery() {
                queryStartDate = DateTime.Now,
                queryEndDate = DateTime.Now,
                optionType = EnumModels.OptionDailyType.MTX
            });
        }
    }
}