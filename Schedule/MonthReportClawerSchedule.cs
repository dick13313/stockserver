using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using StockServer.Clawer;
using StockServer.Repository;

namespace StockServer.Schedule
{
    public class MonthReportClawerSchedule : IInvocable
    {
        private MonthReportClawer _monthReportClawer;
        private MonthReportRepository _monthReportRepository;
        public MonthReportClawerSchedule (MonthReportClawer monthReportClawer, MonthReportRepository monthReportRepository)
        {
            _monthReportClawer = monthReportClawer;
            _monthReportRepository = monthReportRepository;
        }

        public async Task Invoke()
        {
            int year = _monthReportRepository.GetMaxYear();
            int month = _monthReportRepository.GetMaxMonth(year);
            int twNowYear = DateTime.Now.Year-1911;
            while(year <= twNowYear) 
            {
                while((year < twNowYear && month <= 12) || (year == twNowYear && month <= DateTime.Now.Month))
                {
                    await _monthReportClawer.ExecuteAsync(year, month);
                    Thread.Sleep(7000);
                    month++;
                }
                year++;
                month = 1;
            }
        }
    }
}