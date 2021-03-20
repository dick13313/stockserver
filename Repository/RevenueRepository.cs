using System;
using System.Collections.Generic;
using Dapper;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using StockServer.Models.View;

namespace StockServer.Repository
{

    public class RevenueRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<RevenueRepository> _logger;
        public RevenueRepository(ILogger<RevenueRepository> logger, SqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        public IEnumerable<MonthRevenueIncrease> GetMonthRevenueIncrease(int year, int month)
        {
            return _conn.Query<MonthRevenueIncrease>(
@"select stockinfo.stock_id, 
stockinfo.stock_name, 
(CONVERT (decimal, mr1.revenue)/CONVERT (decimal, mr2.revenue)) last_month_increase, 
(CONVERT (decimal, mr1.revenue)/CONVERT (decimal, mr3.revenue)) last_year_increase,
mr1.revenue this_revenue,
mr2.revenue last_month_revenue,
mr3.revenue last_year_revenue
from MonthReport mr1, MonthReport mr2, MonthReport mr3, 
(select distinct stock_id, stock_name from TaiwanStockInfo) stockinfo
where 
mr1.year = @year and mr1.month = @month and
mr2.year = @last_month_year and mr2.month = @last_month and
mr3.year = @last_year and mr3.month = @month and
mr1.stock_id = mr2.stock_id and
mr2.stock_id = mr3.stock_id and
mr1.stock_id = stockinfo.stock_id and
CONVERT (int, mr1.revenue) > CONVERT (int, mr2.revenue) and 
CONVERT (int, mr1.revenue) > CONVERT (int, mr3.revenue) and 
mr1.revenue > 0 and mr2.revenue > 0 and mr3.revenue > 0
order by last_year_increase desc, last_month_increase desc",
                new {
                    year,
                    month,
                    last_year = year - 1,
                    last_month = month == 1 ? 12 : month - 1,
                    last_month_year = month == 1 ? year - 1 : year
                }
            );
        }

        public IEnumerable<SeasonRevenueIncrease> GetSeasonRevenueIncrease(int year, int season)
        {
            return _conn.Query<SeasonRevenueIncrease>(
@"select 
	ThisSeason.*, 
    stockinfo.stock_name,
	LastYearSeason.eps last_year_eps,  
	(Convert(decimal(10,2),ThisSeason.eps) - Convert(decimal(10,2),LastYearSeason.eps))/Convert(decimal(10,2),ThisSeason.eps)*100 increase_eps_rate
from 
(
	SELECT 
		 stock_id,
		[基本每股盈餘（元）]  eps,
		Convert(bigint,replace([營業毛利（毛損）淨額],',','')) gross_profit,
		Convert(bigint,replace([營業收入],',','')) operating_income
	FROM
	(SELECT stock_id, item, value FROM SeasonReport
	 WHERE year =@year and season =@season and type = 1 ) AS SourceTable
	PIVOT
	(
	max(value)
	FOR item IN ( [公司名稱], [基本每股盈餘（元）],[營業毛利（毛損）淨額], [營業收入])
	) AS temp
) ThisSeason,
(
	SELECT 
		 stock_id,
		[基本每股盈餘（元）]  eps,
		Convert(bigint,replace([營業毛利（毛損）淨額],',','')) gross_profit,
		Convert(bigint,replace([營業收入],',','')) operating_income
	FROM
	(SELECT stock_id, item, value FROM SeasonReport
	 WHERE year =@last_year and season =@season and type = 1 ) AS SourceTable
	PIVOT
	(
	max(value)
	FOR item IN ( [公司名稱], [基本每股盈餘（元）],[營業毛利（毛損）淨額], [營業收入])
	) AS temp
) LastYearSeason,
(select distinct stock_id, stock_name from TaiwanStockInfo) stockinfo
where 
	ThisSeason.stock_id = LastYearSeason.stock_id and
    ThisSeason.stock_id = stockinfo.stock_id and
	ThisSeason.eps > LastYearSeason.eps and
	Convert(decimal,ThisSeason.eps) > 0
order by increase_eps_rate desc",
                new {
                    year,
                    season,
                    last_year = year - 1
                }
            );
        }
    }
}