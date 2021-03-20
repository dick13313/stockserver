using System;
using System.Threading.Tasks;
using Coravel.Invocable;
using Microsoft.Extensions.Logging;
using StockServer.Clawer;
using StockServer.Const;
using StockServer.Models.Query;

namespace StockServer.Schedule
{
    public class OptionLegalClawerSchedule : IInvocable
    {
        private readonly ILogger<OptionLegalClawerSchedule> _logger;
        private OptionLegalClawer _optionLegalClawer;
        public OptionLegalClawerSchedule(ILogger<OptionLegalClawerSchedule> logger,OptionLegalClawer optionLegalClawer)
        {
            _logger = logger;
            _optionLegalClawer = optionLegalClawer;
        }
        public async Task Invoke()
        {
            _logger.LogInformation($"OptionLegalClawerSchedule Start");
            await _optionLegalClawer.ExecuteAsync(new OptionLegalQuery() {
                queryStartDate = DateTime.Now,
                queryEndDate = DateTime.Now,
                optionType = EnumModels.OptionLegalType.MXF
            });
        }
    }
}