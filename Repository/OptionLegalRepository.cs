using System;
using System.Collections.Generic;
using System.Transactions;
using Dapper;
using Dapper.Contrib.Extensions;
using Microsoft.Data.SqlClient;
using Microsoft.Extensions.Logging;
using stockserver.Models.View;
using StockServer.Models.DataBase;

namespace StockServer.Repository
{
    public class OptionLegalRepository
    {
        private readonly SqlConnection _conn;
        private readonly ILogger<OptionLegalRepository> _logger;
        public OptionLegalRepository(ILogger<OptionLegalRepository> logger, SqlConnection conn) 
        {
            _logger = logger;
            _conn = conn;
        }

        public void Insert(IEnumerable<OptionLegal> optionLegalList)
        {
             try
            {
                using(var scope = new TransactionScope())
                {
                    foreach(var optionLegal in optionLegalList)
                    {
                        _conn.Insert(optionLegal);
                    }
                    scope.Complete();
                }
            }
            catch(Exception ex)
            {
                _logger.LogError(ex.Message);
            }
        }

        public bool IsExist(OptionLegal optionLegal)
        {
            return _conn.ExecuteScalar<bool>("select 1 from OptionLegal where date=@date and name=@name and legal_name=@legal_name", optionLegal);
        }

        /// <summary>
        /// 三大法人單日買賣超總合
        /// </summary>
        /// <returns></returns>
        public IEnumerable<OptionLegalSummary> GetLegalSummaryList()
        {
            return _conn.Query<OptionLegalSummary>(
@"SELECT date,
    name,
    sum(lot_buy) lot_buy,
    sum(lot_buy_amount) lot_buy_amount,
    sum(lot_sell) lot_sell,
    sum(lot_sell_amount) lot_sell_amount,
    sum(open_interest_buy) open_interest_buy,
    sum(open_interest_buy_amount) open_interest_buy_amount,
    sum(open_interest_sell) open_interest_sell,
    sum(open_interest_sell_amount) open_interest_sell_amount
from OptionLegal group by date, name"
            );
        }

        /// <summary>
        /// 三大法人單日買賣超總合
        /// </summary>
        /// <param name="date"></param>
        /// <param name="name">
        /// 
        /// 臺股期貨 
        /// 電子期貨
        /// 金融期貨
        /// 小型臺指期貨
        /// 臺灣50期貨
        /// 股票期貨
        /// ETF期貨
        /// 櫃買指數期貨
        /// 非金電期貨
        /// 富櫃200期貨
        /// 東證期貨
        /// 美國標普500期貨
        /// 美國那斯達克100期貨
        /// 美國道瓊期貨
        /// </param>
        /// <returns>OptionLegalSummary</returns>
        public OptionLegalSummary GetLegalSummary(DateTime date, string name = "小型臺指期貨")
        {
            return _conn.QuerySingleOrDefault<OptionLegalSummary>(
@"SELECT date,
    name,
    sum(lot_buy) lot_buy,
    sum(lot_buy_amount) lot_buy_amount,
    sum(lot_sell) lot_sell,
    sum(lot_sell_amount) lot_sell_amount,
    sum(open_interest_buy) open_interest_buy,
    sum(open_interest_buy_amount) open_interest_buy_amount,
    sum(open_interest_sell) open_interest_sell,
    sum(open_interest_sell_amount) open_interest_sell_amount
from OptionLegal where date=@date and name=@name group by date, name", 
                new { date, name }
            );
        }
    }
}