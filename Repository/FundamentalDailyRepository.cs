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
    public class FundamentalDailyRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<FundamentalDailyRepository> _logger;
        public FundamentalDailyRepository(ILogger<FundamentalDailyRepository> logger, SqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<FundamentalDaily> fundamentalDailyList)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    foreach (var fundamentalDaily in fundamentalDailyList)
                    {
                        _conn.Insert(fundamentalDaily);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public IEnumerable<FundamentalDaily> GetByDate(DateTime date)
        {
            return _conn.Query<FundamentalDaily>(
                "select * from FundamentalDaily where date=@date", new { date }
            );
        }

        public bool IsExist(DateTime date)
        {
            return _conn.ExecuteScalar<bool>(
                "select count(1) from FundamentalDaily where date=@date",
                new { date }
            );
        }
    }
}