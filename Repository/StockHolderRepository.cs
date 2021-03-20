using System;
using System.Collections.Generic;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using StockServer.Models.DataBase;
using StockServer.Models.View;

namespace StockServer.Repository
{
    public class StockHolderRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<StockHolderRepository> _logger;
        public StockHolderRepository(ILogger<StockHolderRepository> logger, SqlConnection conn)
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<StockHolder> stockHolderList)
        {
            try
            {
                using (var scope = new TransactionScope())
                {
                    foreach (var stockHolder in stockHolderList)
                    {
                        _conn.Insert(stockHolder);
                    }
                    scope.Complete();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public bool IsExist(string stockId, string dateString)
        {
            return _conn.ExecuteScalar<bool>(
                "select count(1) from StockHolder where stock_id=@stockId and date_string=@dateString", 
                new { stockId, dateString}
            );
        }

        public IEnumerable<StockHolderIncrease> GetIncrease(string dateString1, string dateString2)
        {
            return _conn.Query<StockHolderIncrease>(
@"select thisweek.stock_id, stockInfo.stock_name, thisweek.holder_percent thisweek_holder_percent, lastweek.holder_percent lastweek_holder_percent, (thisweek.holder_percent - lastweek.holder_percent) up_percent 
from 
    (
        select (holder.stock_holder_count/total.stock_holder_count*100) holder_percent, total.stock_id
        from
        (select sum(Convert(DECIMAL,stock_holder_count)) stock_holder_count, stock_id, date_string  FROM StockHolder where  date_string=@dateString1 and holder_level > '12' and holder_level <= '15' GROUP BY date_string, stock_id) holder,
        (select sum(Convert(DECIMAL,stock_holder_count)) stock_holder_count, stock_id, date_string  FROM StockHolder where  date_string=@dateString1 and holder_level = 'total' GROUP BY date_string, stock_id) total
        where holder.stock_id = total.stock_id and holder.date_string = total.date_string
    ) thisweek,
    (
        select (holder.stock_holder_count/total.stock_holder_count*100) holder_percent, total.stock_id
        from
        (select sum(Convert(DECIMAL,stock_holder_count)) stock_holder_count, stock_id, date_string  FROM StockHolder where  date_string=@dateString2 and holder_level > '12' and holder_level <= '15' GROUP BY date_string, stock_id) holder,
        (select sum(Convert(DECIMAL,stock_holder_count)) stock_holder_count, stock_id, date_string  FROM StockHolder where  date_string=@dateString2 and holder_level = 'total' GROUP BY date_string, stock_id) total
        where holder.stock_id = total.stock_id and holder.date_string = total.date_string
    ) lastweek,
    (select distinct stock_id, stock_name from TaiwanStockInfo) stockInfo
where thisweek.stock_id = lastweek.stock_id and thisweek.holder_percent > lastweek.holder_percent and thisweek.holder_percent > 50 and stockInfo.stock_id = thisweek.stock_id
order by thisweek.holder_percent - lastweek.holder_percent desc",
                new {
                    dateString1,
                    dateString2
                }
            );
        }
    }
}