using System.Linq;
using System;
using Microsoft.Extensions.Logging;
using StockServer.Models.Query;
using StockServer.Repository;
using System.Collections.Generic;
using StockServer.Models.DataBase;
using StockServer.Models.View;
using stockserver.Models.View;
using AutoMapper;

namespace StockServer.Service
{
    public class StockPickingService
    {
        private readonly ILogger<StockPickingService> _logger;
        private SeasonReportRepository _seasonReportRepository;
        private RevenueRepository _revenueRepository;
        private FundamentalDailyRepository _fundamentalDailyRepository;
        private StockRepository _stockRepository;
        private IMapper _mapper;
        public StockPickingService(
            ILogger<StockPickingService> logger,
            SeasonReportRepository seasonReportRepository,
            RevenueRepository revenueRepository,
            FundamentalDailyRepository fundamentalDailyRepository,
            StockRepository stockRepository,
            IMapper mapper
        )
        {
            _logger = logger;
            _seasonReportRepository = seasonReportRepository;
            _revenueRepository = revenueRepository;
            _fundamentalDailyRepository = fundamentalDailyRepository;
            _stockRepository = stockRepository;
            _mapper = mapper;
        }
        
        /// <summary>
        /// 基本面選股方式篩選:
        /// 1. EPS 大於 前一季EPS
        /// 2. 月營收 大於 上月營收 和 去年同月營收
        /// 3. 本益比 大於 15 , 股價淨值比 小於 2 , 殖利率 大於 4 => GetFundamentalDailyList(date, 15, 2, 4)
        /// 4. 股價 大於 10 , 股價 小於 50
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        public IEnumerable<string> GetPackingStockIds_1(DateTime date)
        {
            var seasonReportList = GetSeasonReportPivotListEPSOverThenLastSeason(date, 1);

            var lastMonthDate = date.AddMonths(-1);
            var monthRevenueIncreaseList = _revenueRepository.GetMonthRevenueIncrease(lastMonthDate.Year - 1911, lastMonthDate.Month);
            
            var fundamentalDailyList = GetFundamentalDailyList(date, 15, 2, 4); 
            return seasonReportList.Select(x => x.stock_id)
                .Intersect(monthRevenueIncreaseList.Select(x => x.stock_id))
                .Intersect(fundamentalDailyList.Select(x => x.stock_id))
                .Where(stock_id => {
                    var stockList = _stockRepository.GetStocks(new StockQuery() {
                        stock_id = stock_id,
                        start_date = date.AddDays(-36),
                        end_date = date
                    }).OrderByDescending(x => x.trading_date).Take(20).OrderBy(x => x.trading_date).ToList();
                    
                    var closeValue = Convert.ToDecimal(stockList.ElementAt(0).close);
                    if(closeValue > 10 && closeValue < 50)
                        return (stockList.Sum(stock => Convert.ToDecimal(stock.close)) / stockList.Count) <  Convert.ToDecimal(stockList.ElementAt(0).close);
                    else
                        return false;
                });//.ToList();
        }

        /// <summary>
        /// 取得當季 EPS 大於 前 N 季 EPS
        /// </summary>
        /// <param name="date"></param>
        /// <param name="lastSeasonCount">前 N 季</param>
        /// <returns></returns>
        public IEnumerable<SeasonReportPivot> GetSeasonReportPivotListEPSOverThenLastSeason(DateTime date, int lastSeasonCount)
        {
            var seasonReportQuery = new SeasonReportQuery(date);
            var seasonReportList = _seasonReportRepository.GetSeasonReportPivotList(seasonReportQuery.year, seasonReportQuery.season);
            for (int i=1; i<=lastSeasonCount;i++) {
                int season = seasonReportQuery.season - i;
                int year = seasonReportQuery.year;
                while(season <= 0) {
                    season += 4;
                    year -= 1;
                }
                var lastSeasonReportList = _seasonReportRepository.GetSeasonReportPivotList(year, season);
                seasonReportList = seasonReportList.Where(report => {
                    var lastReport = lastSeasonReportList.SingleOrDefault(_lastReport => report.stock_id == _lastReport.stock_id);
                    if(lastReport == null)
                        return false;
                    return report.eps > lastReport.eps;
                });
            }
            return seasonReportList;
        }

        /// <summary>
        /// 本益比, 股價淨值比, 殖利率
        /// </summary>
        /// <param name="date"></param>
        /// <param name="pe_ratio">本益比 大於 pe_ratio</param>
        /// <param name="price_book_ratio">股價淨值比 小於 price_book_ratio</param>
        /// <param name="dividend_yield">殖利率 大於 dividend_yield</param>
        /// <returns></returns>
        public IEnumerable<FundamentalDaily> GetFundamentalDailyList(DateTime date, int pe_ratio, int price_book_ratio, int dividend_yield)
        {
            IEnumerable<FundamentalDaily> fundamentalDailyList = null;
            for(int i=0; fundamentalDailyList == null || fundamentalDailyList.Count() == 0; i++)
                fundamentalDailyList = _fundamentalDailyRepository.GetByDate(date.AddDays(-i));
            return fundamentalDailyList.Where(fundamentalDaily => 
                                    fundamentalDaily.pe_ratio < pe_ratio &&
                                    fundamentalDaily.price_book_ratio < price_book_ratio &&
                                    fundamentalDaily.dividend_yield > dividend_yield);
        }

        /// <summary>
        /// 計算多筆股票，在一段時間的平均年化報酬率
        /// </summary>
        /// <param name="stockIds">多筆股票代碼</param>
        /// <param name="startDate">開始日期</param>
        /// <param name="endDate">結束日期</param>
        /// <returns>平均年化報酬率</returns>
        public decimal GetAverageRate(IEnumerable<string> stockIds, DateTime startDate, DateTime endDate)
        {
            return stockIds.Select(stock_id => {
                // var stockList1 = _stockRepository.GetStocks(new StockQuery() {
                //     stock_id = stock_id,
                //     start_date = startDate,
                //     end_date = startDate.AddDays(5)
                // });
                
                // var stockList2 = _stockRepository.GetStocks(new StockQuery() {
                //     stock_id = stock_id,
                //     start_date = endDate.AddDays(-5),
                //     end_date = endDate
                // });
                
                var stockList1 = _stockRepository.GetBiggerThenDateStocks(stock_id, startDate, 1);
                
                var stockList2 = _stockRepository.GetSmallerThenDateStocks(stock_id, endDate, 1);

                var start = Convert.ToDecimal(stockList1.ElementAt(0).close);
                var end = Convert.ToDecimal(stockList2.ElementAt(stockList2.Count() - 1).close);
                return (end / start - 1) * 100;
            }).Sum() / stockIds.Count();
        }
    }
}