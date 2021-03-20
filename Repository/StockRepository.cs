using Microsoft.Data.SqlClient;
using Dapper;
using System.Transactions;
using Dapper.Contrib.Extensions;
using StockServer.Models.DataBase;
using System.Collections.Generic;
using System;
using Microsoft.Extensions.Logging;
using StockServer.Models.Query;

namespace StockServer.Repository
{
    public class StockRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<StockRepository> _logger;
        public StockRepository(ILogger<StockRepository> logger, SqlConnection conn) 
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(List<Stock> stockList)
        {
            try
            {
                using(var scope = new TransactionScope())
                {
                    foreach(var stock in stockList)
                    {
                        _conn.Insert(stock);
                    }
                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public int GetMaxId()
        {
            return _conn.ExecuteScalar<int>("select max(id) id from TWStockPrice_history_1990");
        }

        public DateTime GetMaxDate()
        {
            return _conn.ExecuteScalar<DateTime>("select max(trading_date) id from TWStockPrice_history_1990");
        }

        public IEnumerable<Stock> GetStocks(StockQuery stockQuery)
        {
            return _conn.Query<Stock>(
                "select * from TWStockPrice_history_1990 where stock_id=@stock_id and trading_date >= @start_date and trading_date <= @end_date order by trading_date",
                stockQuery
            );
        }

        public IEnumerable<Stock> GetBiggerThenDateStocks(string stock_id, DateTime date, int days)
        {
            return _conn.Query<Stock>(
                "select top (@days) * from TWStockPrice_history_1990 where stock_id=@stock_id and trading_date >= @date order by trading_date",
                new {
                    stock_id,
                    date,
                    days
                }
            );
        }

        public IEnumerable<Stock> GetSmallerThenDateStocks(string stock_id, DateTime date, int days)
        {
            return _conn.Query<Stock>(
@"
select t1.* from (
    select top (@days) * from TWStockPrice_history_1990 where stock_id=@stock_id and trading_date <= @date order by trading_date desc
) t1 order by t1.trading_date",
                new {
                    stock_id,
                    date,
                    days
                }
            );
        }
    }
}