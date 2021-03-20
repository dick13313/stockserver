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
    public class StockDividendRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<StockDividendRepository> _logger;
        public StockDividendRepository(ILogger<StockDividendRepository> logger, SqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<StockDividend> stockDividendList)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    foreach (var stockDividend in stockDividendList)
                    {
                        _conn.Insert(stockDividend);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public bool IsExist(StockDividend stockDividend)
        {
            return _conn.ExecuteScalar<bool>(
                "select count(1) from StockDividend where stock_id=@stock_id and time_string=@time_string",
                stockDividend
            );
        }
    }
}