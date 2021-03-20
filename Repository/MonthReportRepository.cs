using System;
using System.Collections.Generic;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;

namespace StockServer.Repository
{
    public class MonthReportRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<MonthReportRepository> _logger;
        public MonthReportRepository(ILogger<MonthReportRepository> logger, SqlConnection conn) 
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<MonthReport> monthReportList)
        {
            try
            {
                using(var scope = new TransactionScope())
                {
                    foreach(var month in monthReportList)
                    {
                        _conn.Insert(month);
                    }
                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        internal bool IsExist(int year, int month)
        {
            return _conn.ExecuteScalar<bool>("select count(1) from MonthReport where year=@year and month=@month", new { year, month });
        }

        public int GetMaxMonth(int year)
        {
            return _conn.ExecuteScalar<int>("select isnull(max(month), 1) from MonthReport where year=@year", new { year });
        }

        public int GetMaxYear()
        {
            return _conn.ExecuteScalar<int>("select isnull(max(year), 102) from MonthReport");
        }
    }
}