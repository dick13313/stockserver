using System;
using System.Collections.Generic;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using StockServer.Const;
using StockServer.Models.DataBase;
using StockServer.Models.View;

namespace StockServer.Repository
{
    public class SeasonReportRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<SeasonReportRepository> _logger;
        public SeasonReportRepository(ILogger<SeasonReportRepository> logger, SqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<SeasonReport> seasionReportList)
        {
            try
            {
                using (var scope = new TransactionScope(TransactionScopeOption.Required, new TimeSpan(0, 5, 0)))
                {
                    foreach (var seasionReport in seasionReportList)
                    {
                        _conn.Insert(seasionReport);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public int GetMaxYear(EnumModels.SeasonReportType seasonReportType)
        {
            return _conn.ExecuteScalar<int>(
                "select isnull(max(year), 106) from SeasonReport where type=@type",
                new { type = ((int)seasonReportType) }
            );
        }

        public int GetMaxSeason(EnumModels.SeasonReportType seasonReportType, int year)
        {
            return _conn.ExecuteScalar<int>(
                "select isnull(max(season), 1) from SeasonReport where year=@year and type=@type",
                new { year, type = ((int)seasonReportType) }
            );
        }

        public bool IsExist(EnumModels.SeasonReportType seasonReportType, int year, int season)
        {
            return _conn.ExecuteScalar<bool>(
                "select count(1) from SeasonReport where year=@year and season=@season and type=@type",
                new { 
                    year,
                    season,
                    type = ((int)seasonReportType) 
                }
            );
        }

        public IEnumerable<SeasonReportPivot> GetSeasonReportPivotList(int year, int season)
        {
            return _conn.Query<SeasonReportPivot>(
@"SELECT 
	 stock_id,
	[基本每股盈餘（元）]  eps,
	Convert(bigint,replace([營業毛利（毛損）淨額],',','')) gross_profit,
	Convert(bigint,replace([營業收入],',','')) operating_income
FROM
    (SELECT stock_id, item, value FROM SeasonReport
    WHERE year = @year and season = @season and type = 1 ) AS SourceTable
PIVOT
(
    max(value)
    FOR item IN ( [公司名稱], [基本每股盈餘（元）],[營業毛利（毛損）淨額], [營業收入])
) AS PivotTable
WHERE [營業收入] != '--' and [營業毛利（毛損）淨額] != '--'",
                new { year, season }
            );
        }

        public SeasonReportPivot GetSeasonReportPivot(string stock_id, int year, int season)
        {
            return _conn.QuerySingleOrDefault<SeasonReportPivot>(
@"SELECT 
	 stock_id,
	[基本每股盈餘（元）]  eps,
	Convert(bigint,replace([營業毛利（毛損）淨額],',','')) gross_profit,
	Convert(bigint,replace([營業收入],',','')) operating_income
FROM
    (SELECT stock_id, item, value FROM SeasonReport
    WHERE year = @year and season = @season and type = 1 ) AS SourceTable
PIVOT
(
    max(value)
    FOR item IN ( [公司名稱], [基本每股盈餘（元）],[營業毛利（毛損）淨額], [營業收入])
) AS PivotTable
WHERE stock_id=@stock_id and 
[營業收入] != '--' and [營業毛利（毛損）淨額] != '--'",
                new { stock_id, year, season }
            );
        }
    }
}