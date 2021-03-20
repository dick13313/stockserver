using System;
using System.Threading;
using System.Threading.Tasks;
using Coravel.Invocable;
using StockServer.Clawer;
using StockServer.Const;
using StockServer.Repository;

namespace StockServer.Schedule
{
    public class SeasonReportClawerSchedule : IInvocable
    {
        private SeasonReportClawer _seasonReportClawer;
        private SeasonReportRepository _seasonReportRepository;
        public SeasonReportClawerSchedule (SeasonReportClawer seasonReportClawer, SeasonReportRepository seasonReportRepository)
        {
            _seasonReportClawer = seasonReportClawer;
            _seasonReportRepository = seasonReportRepository;
        }

        /*
            ●第一季(Q1)財報：5/15前
            ●第二季(Q2)財報：8/14前
            ●第三季(Q3)財報：11/14前
            ●第四季(Q4)財報及年報：隔年3/31前
        */
        public async Task Invoke()
        {
            foreach(EnumModels.SeasonReportType seasonReportType in Enum.GetValues(typeof(EnumModels.SeasonReportType)))
            {
                int year =  _seasonReportRepository.GetMaxYear(seasonReportType);
                int season = _seasonReportRepository.GetMaxSeason(seasonReportType, year);
                while(year <=  DateTime.Now.Year-1911) {
                    while(season <= 4)
                    {
                        await _seasonReportClawer.ExecuteAsync(seasonReportType, year, season);
                        Thread.Sleep(7000);
                        season++;
                    }
                    year++;
                    season = 1;
                }
            }
        }
    }
}